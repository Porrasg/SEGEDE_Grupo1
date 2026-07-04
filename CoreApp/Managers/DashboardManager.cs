using SEGEDE_Grupo1.DataAccess.CRUD;
using SEGEDE_Grupo1.EntitiesDTOs.Constants;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;
using SEGEDE_Grupo1.EntitiesDTOs.Helpers;

namespace SEGEDE_Grupo1.CoreApp.Managers;

// Manager de Dashboards y KPIs (§14.13). Instanciación directa con new sin IoC.
// Genera respuestas agregadas para los paneles de control de Administrador, Ingeniero (Operaciones) y Comprador (con validación de ownership).
public class DashboardManager
{
    private readonly TurbineCrudFactory _turbineFactory = new();
    private readonly CentralBankCrudFactory _cbFactory = new();
    private readonly ForecastCrudFactory _forecastFactory = new();
    private readonly AccountStatementCrudFactory _statementFactory = new();
    private readonly FlushCrudFactory _flushFactory = new();
    private readonly MaintenanceCrudFactory _maintenanceFactory = new();
    private readonly DistributionDetailCrudFactory _detailFactory = new();

    // RF-067: Retorna KPIs totales para el panel de Administrador.
    public DashboardAdminResponse GetAdminDashboard()
    {
        var now = TimeHelper.NowCR();
        var turbines = _turbineFactory.RetrieveAll<Turbine>();
        var cb = _cbFactory.RetrieveSingleton();
        var forecasts = _forecastFactory.RetrieveByMonth(now.Month, now.Year)
            .Where(f => !string.Equals(f.Status, ForecastStates.Cancelled, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var statements = _statementFactory.RetrieveAll<AccountStatement>()
            .Where(s => s.Month == now.Month && s.Year == now.Year &&
                        !string.Equals(s.Status, StatementStates.Annulled, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var lastFlush = _flushFactory.RetrieveAll<Flush>()
            .Where(f => string.Equals(f.Status, FlushStates.Completed, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(f => f.EndDate ?? f.StartDate)
            .FirstOrDefault();

        return new DashboardAdminResponse
        {
            TotalTurbines = turbines.Count,
            ActiveTurbines = turbines.Count(t => string.Equals(t.Status, TurbineStates.Active, StringComparison.OrdinalIgnoreCase)),
            CentralBankInventory = cb?.CurrentInventory ?? 0m,
            EffectiveCapacity = cb?.EffectiveCapacity ?? 0m,
            MonthForecasts = forecasts.Count,
            MonthTotalDemand = forecasts.Sum(f => f.AmountMWh),
            MonthTotalBilled = statements.Sum(s => s.Total),
            LastFlush = lastFlush?.EndDate ?? lastFlush?.StartDate
        };
    }

    // RF-068 / RN-030: Retorna KPIs operativos para el panel de Ingeniero. Nunca expone datos financieros.
    public DashboardOperationsResponse GetOperationsDashboard()
    {
        var now = TimeHelper.NowCR();
        var turbines = _turbineFactory.RetrieveAll<Turbine>();
        var cb = _cbFactory.RetrieveSingleton();
        var lastFlush = _flushFactory.RetrieveAll<Flush>()
            .Where(f => string.Equals(f.Status, FlushStates.Completed, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(f => f.EndDate ?? f.StartDate)
            .FirstOrDefault();
        var maintenances = _maintenanceFactory.RetrieveAll<Maintenance>();

        int overdueAlerts = maintenances.Count(m =>
            m.EstimatedStartDate < now &&
            (string.Equals(m.Status, MaintenanceStates.Scheduled, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(m.Status, MaintenanceStates.InProgress, StringComparison.OrdinalIgnoreCase)));

        return new DashboardOperationsResponse
        {
            TotalTurbines = turbines.Count,
            ActiveTurbines = turbines.Count(t => string.Equals(t.Status, TurbineStates.Active, StringComparison.OrdinalIgnoreCase)),
            TurbinesUnderMaintenance = turbines.Count(t => string.Equals(t.Status, TurbineStates.UnderMaintenance, StringComparison.OrdinalIgnoreCase)),
            DamagedTurbines = turbines.Count(t => string.Equals(t.Status, TurbineStates.Damaged, StringComparison.OrdinalIgnoreCase)),
            SuspendedTurbines = turbines.Count(t => string.Equals(t.Status, TurbineStates.SuspendedForNonCompliance, StringComparison.OrdinalIgnoreCase)),
            CentralBankInventory = cb?.CurrentInventory ?? 0m,
            LastFlushDate = lastFlush?.EndDate ?? lastFlush?.StartDate,
            LastFlushEnergy = lastFlush?.TotalTransferredEnergy ?? 0m,
            OverdueMaintenanceAlerts = overdueAlerts
        };
    }

    // RF-069: Retorna KPIs propios para el panel del Comprador. Las capas superiores verifican el rol/identidad de la sesión antes de invocar este método.
    public DashboardBuyerResponse GetBuyerDashboard(int callerUserId)
    {
        var now = TimeHelper.NowCR();
        var buyerForecasts = _forecastFactory.RetrieveByBuyer(callerUserId);
        var activeForecasts = buyerForecasts.Where(f =>
            !string.Equals(f.Status, ForecastStates.Cancelled, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(f.Status, ForecastStates.Distributed, StringComparison.OrdinalIgnoreCase)).ToList();

        var monthReq = buyerForecasts.Where(f => f.Month == now.Month && f.Year == now.Year &&
            !string.Equals(f.Status, ForecastStates.Cancelled, StringComparison.OrdinalIgnoreCase)).Sum(f => f.AmountMWh);

        var details = _detailFactory.RetrieveByBuyer(callerUserId);
        var lastAssignment = details.OrderByDescending(d => d.Created).FirstOrDefault()?.AssignedMWh ?? 0m;

        var statements = _statementFactory.RetrieveByBuyer(callerUserId)
            .Where(s => !string.Equals(s.Status, StatementStates.Annulled, StringComparison.OrdinalIgnoreCase))
            .ToList();

        decimal totalBilled = statements.Sum(s => s.Total);
        var lastStmtDate = statements.OrderByDescending(s => s.IssueDate).FirstOrDefault()?.IssueDate;

        return new DashboardBuyerResponse
        {
            ActiveForecasts = activeForecasts.Count,
            MonthRequestedMWh = monthReq,
            LastAssignment = lastAssignment,
            TotalBilledAccumulated = totalBilled,
            LastStatementDate = lastStmtDate
        };
    }
}
