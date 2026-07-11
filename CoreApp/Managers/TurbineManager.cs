using SEGEDE_Grupo1.DataAccess.CRUD;
using SEGEDE_Grupo1.EntitiesDTOs.Constants;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;
using SEGEDE_Grupo1.EntitiesDTOs.Exceptions;
using SEGEDE_Grupo1.EntitiesDTOs.Helpers;
using SEGEDE_Grupo1.EntitiesDTOs.Validation;

namespace SEGEDE_Grupo1.CoreApp.Managers;

// Manager de Turbinas (§14.2). Instanciación directa con new sin IoC.
// Gestiona registro (con creación de batería local), actualización, transiciones de estado, historial, métricas y verificación de mantenimiento vencido.
public class TurbineManager
{
    private readonly TurbineCrudFactory _turbineCrudFactory = new();
    private readonly TurbineStateHistoryCrudFactory _stateHistoryCrudFactory = new();
    private readonly LocalBatteryCrudFactory _localBatteryCrudFactory = new();
    private readonly CentralBankCrudFactory _centralBankCrudFactory = new();
    private readonly MaintenanceCrudFactory _maintenanceCrudFactory = new();
    private readonly FailureCrudFactory _failureCrudFactory = new();
    private readonly EnergyGenerationLogCrudFactory _genLogCrudFactory = new();
    private readonly EnergyLossLogCrudFactory _lossLogCrudFactory = new();
    private readonly NotificationQueueCrudFactory _notificationFactory = new();
    private readonly AuditManager _auditManager = new();

    // RF-013: Registro de turbina. Crea turbina, su batería local 1:1, log de estado inicial y recalcula capacidad del Banco Central.
    public void Register(RegisterTurbineRequest r, int callerUserId)
    {
        TurbineValidator.Validate(r.UniqueCode, r.Name, r.Location, r.Brand, r.Model, r.Year, r.WeeklyNominalCapacity).ThrowIfInvalid();

        var existing = _turbineCrudFactory.RetrieveByCode(r.UniqueCode);
        if (existing != null)
            throw new BusinessException("A turbine with this unique code already exists.", "DUPLICATE_TURBINE_CODE");

        var now = TimeHelper.NowCR();
        var turbine = new Turbine
        {
            UniqueCode = r.UniqueCode,
            Name = r.Name,
            Location = r.Location,
            Brand = r.Brand,
            Model = r.Model,
            Year = r.Year,
            WeeklyNominalCapacity = r.WeeklyNominalCapacity,
            Status = TurbineStates.Active,
            LastMaintenance = null,
            LastStateChange = now,
            Created = now
        };

        _turbineCrudFactory.Create(turbine);
        var created = _turbineCrudFactory.RetrieveByCode(r.UniqueCode) ?? throw new BusinessException("Failed to retrieve created turbine.");

        var battery = new LocalBattery
        {
            TurbineId = created.Id,
            StoredEnergy = 0m,
            Created = now
        };
        _localBatteryCrudFactory.Create(battery);

        var stateLog = new TurbineStateHistory
        {
            TurbineId = created.Id,
            PreviousState = "None",
            NewState = TurbineStates.Active,
            ChangeDate = now,
            Reason = "Initial Registration",
            UserId = callerUserId,
            Created = now
        };
        _stateHistoryCrudFactory.Create(stateLog);

        RecalculateCentralBankCapacity();

        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.Turbines, AuditActions.Create, "tblTurbines", created.Id, null, $"Registered turbine {created.UniqueCode}");
    }

    // RF-014: Actualización de campos editables de una turbina.
    public void Update(UpdateTurbineRequest r, int callerUserId)
    {
        var existing = _turbineCrudFactory.RetrieveById<Turbine>(r.TurbineId) ?? throw new NotFoundException("Turbine not found.");

        TurbineValidator.Validate(existing.UniqueCode, r.Name, r.Location, r.Brand, r.Model, existing.Year, r.WeeklyNominalCapacity).ThrowIfInvalid();

        existing.Name = r.Name;
        existing.Location = r.Location;
        existing.Brand = r.Brand;
        existing.Model = r.Model;
        existing.WeeklyNominalCapacity = r.WeeklyNominalCapacity;
        existing.Updated = TimeHelper.NowCR();

        _turbineCrudFactory.Update(existing);

        RecalculateCentralBankCapacity();

        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.Turbines, AuditActions.Update, "tblTurbines", existing.Id, null, $"Updated turbine {existing.UniqueCode}");
    }

    // RF-015/016: Cambio de estado de una turbina. Valida transición permitida y recalcula capacidad.
    public void ChangeState(ChangeTurbineStateRequest r, int callerUserId)
    {
        var turbine = _turbineCrudFactory.RetrieveById<Turbine>(r.TurbineId) ?? throw new NotFoundException("Turbine not found.");

        if (!StateTransition.IsValid(turbine.Status, r.NewState))
        {
            throw new BusinessException($"Invalid state transition from {turbine.Status} to {r.NewState}.", "INVALID_STATE_TRANSITION");
        }

        var now = TimeHelper.NowCR();
        string oldState = turbine.Status;

        _turbineCrudFactory.UpdateStatus(turbine.Id, r.NewState, now, turbine.LastMaintenance, now);

        var stateLog = new TurbineStateHistory
        {
            TurbineId = turbine.Id,
            PreviousState = oldState,
            NewState = r.NewState,
            ChangeDate = now,
            Reason = string.IsNullOrWhiteSpace(r.Reason) ? "Manual state change" : r.Reason,
            UserId = callerUserId,
            Created = now
        };
        _stateHistoryCrudFactory.Create(stateLog);

        if (oldState == TurbineStates.Active || r.NewState == TurbineStates.Active)
        {
            RecalculateCentralBankCapacity();
        }

        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.Turbines, AuditActions.Update, "tblTurbines", turbine.Id, oldState, r.NewState);
    }

    // Retorna todas las turbinas registradas.
    public List<Turbine> RetrieveAll()
    {
        return _turbineCrudFactory.RetrieveAll<Turbine>();
    }

    // Retorna una turbina por su ID.
    public Turbine RetrieveById(int id)
    {
        return _turbineCrudFactory.RetrieveById<Turbine>(id) ?? throw new NotFoundException("Turbine not found.");
    }

    // RF-021/022: Retorna el historial completo de la turbina (estados, mantenimientos, fallas y energía total).
    public TurbineHistoryResponse RetrieveHistory(int turbineId)
    {
        var turbine = _turbineCrudFactory.RetrieveById<Turbine>(turbineId) ?? throw new NotFoundException("Turbine not found.");

        var genSum = _genLogCrudFactory.RetrieveSumByTurbine(turbineId);
        decimal lostSum = _lossLogCrudFactory.RetrieveSumByTurbine(turbineId);

        return new TurbineHistoryResponse
        {
            TurbineId = turbineId,
            StateChanges = _stateHistoryCrudFactory.RetrieveByTurbine(turbineId),
            Maintenances = _maintenanceCrudFactory.RetrieveByTurbine(turbineId),
            Failures = _failureCrudFactory.RetrieveByTurbine(turbineId),
            TotalGeneratedEnergy = genSum.TotalGeneratedEnergy,
            TotalLostEnergy = lostSum
        };
    }

    // RF-023: Retorna métricas operacionales (DO, IO, MTBF, MTTR).
    public TurbineMetricsResponse RetrieveMetrics(int turbineId)
    {
        var turbine = _turbineCrudFactory.RetrieveById<Turbine>(turbineId) ?? throw new NotFoundException("Turbine not found.");

        var genSum = _genLogCrudFactory.RetrieveSumByTurbine(turbineId);
        var lossLogs = _lossLogCrudFactory.RetrieveByTurbine(turbineId);
        var failures = _failureCrudFactory.RetrieveByTurbine(turbineId);
        var maintenances = _maintenanceCrudFactory.RetrieveByTurbine(turbineId);

        decimal totalActiveSec = genSum.TotalActiveSeconds;
        decimal totalInactiveSec = lossLogs.Sum(x => x.InactiveTimeSeconds);
        decimal totalSec = totalActiveSec + totalInactiveSec;

        decimal availability = totalSec > 0 ? (totalActiveSec / totalSec) * 100m : 100m;
        decimal unavailability = totalSec > 0 ? (totalInactiveSec / totalSec) * 100m : 0m;

        int totalFailures = failures.Count;
        int totalMaintenances = maintenances.Count;

        decimal mtbf = totalFailures > 0 ? (totalActiveSec / totalFailures) : totalActiveSec;
        decimal mttr = totalFailures > 0 ? (totalInactiveSec / totalFailures) : 0m;

        return new TurbineMetricsResponse
        {
            TurbineId = turbineId,
            TotalActiveSeconds = totalActiveSec,
            TotalInactiveSeconds = totalInactiveSec,
            TotalSeconds = totalSec,
            OperationalAvailability = Math.Round(availability, 2),
            OperationalUnavailability = Math.Round(unavailability, 2),
            TotalFailures = totalFailures,
            TotalMaintenances = totalMaintenances,
            MTBF = Math.Round(mtbf, 2),
            MTTR = Math.Round(mttr, 2)
        };
    }

    // RF-018: Verificación de mantenimiento vencido (> 40 días). Suspende la turbina por incumplimiento y recalcula capacidad.
    public void CheckOverdueMaintenance()
    {
        var threshold = TimeHelper.NowCR().AddDays(-40);
        var overdueTurbines = _turbineCrudFactory.RetrieveOverdue(threshold);

        foreach (var t in overdueTurbines)
        {
            if (t.Status != TurbineStates.SuspendedForNonCompliance && t.Status != TurbineStates.Decommissioned)
            {
                var now = TimeHelper.NowCR();
                string oldState = t.Status;
                _turbineCrudFactory.UpdateStatus(t.Id, TurbineStates.SuspendedForNonCompliance, now, t.LastMaintenance, now);

                _stateHistoryCrudFactory.Create(new TurbineStateHistory
                {
                    TurbineId = t.Id,
                    PreviousState = oldState,
                    NewState = TurbineStates.SuspendedForNonCompliance,
                    ChangeDate = now,
                    Reason = "Suspended due to overdue maintenance (> 40 days)",
                    UserId = 0, // System
                    Created = now
                });

                var notif = new NotificationQueue
                {
                    UserId = 0,
                    RecipientEmail = "admin@segede.local",
                    NotificationType = NotificationTypes.Suspension,
                    Subject = $"Turbine Suspended: {t.UniqueCode}",
                    Body = $"Turbine {t.Name} ({t.UniqueCode}) has been suspended due to lack of maintenance for more than 40 days.",
                    IsCritical = true,
                    Status = NotificationStates.Pending,
                    Attempts = 0,
                    NextAttempt = now,
                    Created = now
                };
                _notificationFactory.Create(notif);

                _auditManager.LogAction(null, "System", AuditModules.Turbines, AuditActions.Block, "tblTurbines", t.Id, oldState, TurbineStates.SuspendedForNonCompliance);
            }
        }

        if (overdueTurbines.Any())
        {
            RecalculateCentralBankCapacity();
        }
    }

    // Recalcula la capacidad automática del Banco Central sumando la capacidad nominal semanal de todas las turbinas activas.
    public void RecalculateCentralBankCapacity()
    {
        var activeTurbines = _turbineCrudFactory.RetrieveAllActive();
        decimal totalCapacity = activeTurbines.Sum(t => t.WeeklyNominalCapacity);
        _centralBankCrudFactory.UpdateAutomaticCapacity(totalCapacity, TimeHelper.NowCR());
    }
}
