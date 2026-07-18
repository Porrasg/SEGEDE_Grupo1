using System.Data;
using SEGEDE_Grupo1.DataAccess.CRUD;
using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

namespace SEGEDE_Grupo1.CoreApp;

// Manager de Distribución Comercial (§14.9). Instanciación directa con new sin IoC.
// Ejecuta el ciclo mensual de distribución de energía bajo semántica ACID (§17.2), manejando escenarios de demanda, inventario, cálculos financieros y notificaciones.
public class DistributionManager
{
    private readonly CommercialDistributionCrudFactory _commDistFactory = new();
    private readonly DistributionDetailCrudFactory _detailFactory = new();
    private readonly ForecastCrudFactory _forecastFactory = new();
    private readonly CentralBankCrudFactory _cbFactory = new();
    private readonly CentralBankLogCrudFactory _cbLogFactory = new();
    private readonly AccountStatementCrudFactory _statementFactory = new();
    private readonly PriceCrudFactory _priceFactory = new();
    private readonly TaxCrudFactory _taxFactory = new();
    private readonly NotificationQueueCrudFactory _notifFactory = new();
    private readonly UserCrudFactory _userFactory = new();
    private readonly AuditManager _auditManager = new();

    // RF-050 a RF-056 (§17.2): Ejecuta el ciclo mensual de distribución comercial. Actor System.
    // Valida inventario del Banco Central, asigna energía proporcionalmente en escasez, genera estados de cuenta y encola notificaciones.
    public void RunMonthlyDistribution(int month, int year)
    {
        var existingDist = _commDistFactory.RetrieveByMonth(month, year);
        if (existingDist != null)
        {
            throw new BusinessException($"Commercial distribution for {month}/{year} has already been executed.", "DISTRIBUTION_ALREADY_EXECUTED");
        }

        var forecasts = _forecastFactory.RetrieveByMonth(month, year)
            .Where(f => !string.Equals(f.Status, ForecastStates.Cancelled, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(f.Status, ForecastStates.Blocked, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(f.Status, ForecastStates.Distributed, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var cb = _cbFactory.RetrieveSingleton() ?? throw new NotFoundException("Central Bank not found.");
        decimal ia = cb.CurrentInventory;
        decimal dt = forecasts.Sum(f => f.AmountMWh);

        string scenario;
        if (forecasts.Count == 0 || dt == 0)
        {
            scenario = DistributionScenarios.ZeroDemand;
        }
        else if (ia == 0)
        {
            scenario = DistributionScenarios.ZeroInventory;
        }
        else if (ia >= dt)
        {
            scenario = DistributionScenarios.Sufficient;
        }
        else
        {
            scenario = DistributionScenarios.Shortage;
        }

        var tExec = TimeHelper.NowCR();

        // Ciclo ACID real (§37.25/§61.2): una única transacción con aislamiento Serializable envuelve
        // la creación de la distribución, sus detalles, los estados de cuenta, el ajuste del Banco Central
        // y el bloqueo de los forecasts del mes. Ante cualquier fallo, se revierte todo el lote.
        using var conn = SqlDao.GetInstance().GetOpenConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            if (scenario == DistributionScenarios.ZeroDemand)
            {
                var abortDist = new CommercialDistribution
                {
                    Month = month,
                    Year = year,
                    ExecutionDate = tExec,
                    AvailableInventory = ia,
                    TotalDemand = dt,
                    DistributedEnergy = 0m,
                    RoundingResidual = ia,
                    Scenario = scenario,
                    Created = tExec
                };
                _commDistFactory.Create(abortDist, conn, tx);
                tx.Commit();
                _auditManager.LogAction(null, SystemActor.Name, AuditModules.Distribution, AuditActions.Execute, "tblCommercialDistribution", 0, null, $"Distribution aborted: {scenario}");
                return;
            }

            var currentPrice = _priceFactory.RetrieveActive();
            if (currentPrice == null)
            {
                throw new BusinessException("No active price configured for commercial distribution.", "MISSING_PRICE");
            }

            var currentTax = _taxFactory.RetrieveActive();
            if (currentTax == null)
            {
                throw new BusinessException("No active tax configured for commercial distribution.", "MISSING_TAX");
            }

            decimal unitPrice = currentPrice.PriceCRCPerMWh;
            decimal taxPercentage = currentTax.Percentage;

            decimal edt = 0m;
            var assignments = new List<(Forecast forecast, decimal ai, decimal dns, decimal subtotal, decimal taxAmount, decimal total)>();

            foreach (var f in forecasts)
            {
                decimal ai = 0m;
                if (scenario == DistributionScenarios.Sufficient)
                {
                    ai = f.AmountMWh;
                }
                else if (scenario == DistributionScenarios.Shortage)
                {
                    ai = Math.Round((f.AmountMWh / dt) * ia, 4);
                }
                else if (scenario == DistributionScenarios.ZeroInventory)
                {
                    ai = 0m;
                }

                decimal dns = f.AmountMWh - ai;
                decimal subtotal = Math.Round(ai * unitPrice, 2);
                decimal taxAmount = Math.Round(subtotal * taxPercentage, 2);
                decimal total = subtotal + taxAmount;

                edt += ai;
                assignments.Add((f, ai, dns, subtotal, taxAmount, total));
            }

            decimal rr = ia - edt;

            var dist = new CommercialDistribution
            {
                Month = month,
                Year = year,
                ExecutionDate = tExec,
                AvailableInventory = ia,
                TotalDemand = dt,
                DistributedEnergy = edt,
                RoundingResidual = rr,
                Scenario = scenario,
                Created = tExec
            };

            _commDistFactory.Create(dist, conn, tx);
            var createdDist = _commDistFactory.RetrieveByMonth(month, year, conn, tx) ?? throw new BusinessException("Failed to retrieve created commercial distribution.");

            _forecastFactory.BlockMonth(month, year, tExec, conn, tx);

            foreach (var item in assignments)
            {
                var detail = new DistributionDetail
                {
                    DistributionId = createdDist.Id,
                    BuyerId = item.forecast.BuyerId,
                    ForecastId = item.forecast.Id,
                    RequestedMWh = item.forecast.AmountMWh,
                    AssignedMWh = item.ai,
                    UnsuppliedDemand = item.dns,
                    Created = tExec
                };
                _detailFactory.Create(detail, conn, tx);

                var stmt = new AccountStatement
                {
                    BuyerId = item.forecast.BuyerId,
                    DistributionId = createdDist.Id,
                    ForecastId = item.forecast.Id,
                    Month = month,
                    Year = year,
                    AssignedMWh = item.ai,
                    UnitPrice = unitPrice,
                    TaxPercentage = taxPercentage,
                    Subtotal = item.subtotal,
                    TaxAmount = item.taxAmount,
                    Total = item.total,
                    Status = StatementStates.Issued,
                    RevisionNumber = 0,
                    ParentId = null,
                    AnnulmentReason = null,
                    IssueDate = tExec,
                    Created = tExec
                };
                _statementFactory.Create(stmt, conn, tx);

                _forecastFactory.UpdateStatus(item.forecast.Id, ForecastStates.Distributed, tExec, conn, tx);

                var buyer = _userFactory.RetrieveById<User>(item.forecast.BuyerId);
                if (buyer != null && !string.IsNullOrWhiteSpace(buyer.Email))
                {
                    var notif = new NotificationQueue
                    {
                        UserId = buyer.Id,
                        RecipientEmail = buyer.Email,
                        NotificationType = NotificationTypes.AccountStatement,
                        Subject = $"Account Statement Issued - {month}/{year}",
                        Body = $"Your account statement for {month}/{year} has been generated. Assigned Energy: {item.ai} MWh. Total Amount: {item.total:C}.",
                        IsCritical = false,
                        Status = NotificationStates.Pending,
                        Attempts = 0,
                        NextAttempt = tExec,
                        SentDate = null,
                        Created = tExec
                    };
                    _notifFactory.Create(notif, conn, tx);
                }
            }

            _cbFactory.UpdateInventory(rr, tExec, conn, tx);

            if (edt > 0)
            {
                var cbLog = new CentralBankLog
                {
                    MovementType = MovementTypes.Outflow,
                    Amount = edt,
                    ResultingInventory = rr,
                    FlushId = null,
                    DistributionId = createdDist.Id,
                    EventDate = tExec,
                    Created = tExec
                };
                _cbLogFactory.Create(cbLog, conn, tx);
            }

            tx.Commit();

            _auditManager.LogAction(null, SystemActor.Name, AuditModules.Distribution, AuditActions.Execute, "tblCommercialDistribution", createdDist.Id, null, $"Executed distribution {month}/{year} ({scenario})");
        }
        catch (Exception ex)
        {
            try { tx.Rollback(); } catch { /* la conexión pudo cerrarse antes del rollback */ }
            _auditManager.LogAction(null, SystemActor.Name, AuditModules.Distribution, AuditActions.Execute, "tblCommercialDistribution", 0, null, $"Distribution failed for {month}/{year}: {ex.Message}");
            throw;
        }
    }

    // Retorna el historial de distribuciones comerciales (requiere rol de Administrador u Operador según capa superior).
    public List<CommercialDistribution> RetrieveHistory()
    {
        return _commDistFactory.RetrieveAll<CommercialDistribution>();
    }

    // Retorna el detalle de asignaciones por comprador de una distribución específica (Admin/Distribution, v2 §85).
    // Ruta faltante detectada al wireear la página — DistributionDetailCrudFactory.RetrieveByDistribution ya existía sin manager/endpoint.
    public List<DistributionDetail> RetrieveDetailByDistribution(int distributionId)
    {
        return _detailFactory.RetrieveByDistribution(distributionId);
    }

    // RF-049: Retorna el detalle de distribución por comprador con validación de ownership.
    public List<DistributionDetail> RetrieveDetailByBuyer(int buyerId, int callerUserId, string callerRole)
    {
        if (!string.Equals(callerRole, "Administrator", StringComparison.OrdinalIgnoreCase) && buyerId != callerUserId)
        {
            throw new UnauthorizedAccessAppException("You can only view your own distribution details.", "OWNERSHIP_VIOLATION");
        }

        return _detailFactory.RetrieveByBuyer(buyerId);
    }
}
