using SEGEDE_Grupo1.DataAccess.CRUD;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

namespace SEGEDE_Grupo1.CoreApp;

// Manager de Mantenimientos (§14.3). Instancia fábricas directamente con new sin IoC.
// Aplica RN-010 (simultaneidad de mantenimientos preventivos en la red) y gestiona transiciones de estado de turbinas asociadas.
public class MaintenanceManager
{
    private readonly MaintenanceCrudFactory _maintenanceCrudFactory = new();
    private readonly TurbineCrudFactory _turbineCrudFactory = new();
    private readonly AuditManager _auditManager = new();

    // RF-017/019: Registra un nuevo mantenimiento. Valida simultaneidad preventiva y cambia estado de turbina a UnderMaintenance.
    public void Register(RegisterMaintenanceRequest r, int callerUserId)
    {
        ValidateMaintenanceInput(r.MaintenanceType, r.EstimatedStartDate, r.EstimatedEndDate);

        var turbine = _turbineCrudFactory.RetrieveById<Turbine>(r.TurbineId) ?? throw new NotFoundException("Turbine not found.");

        if (string.Equals(r.MaintenanceType, MaintenanceTypes.Preventive, StringComparison.OrdinalIgnoreCase))
        {
            var activePrev = CheckActivePreventive();
            if (activePrev != null)
            {
                throw new BusinessException("Another preventive maintenance is already active in the network (RN-010).", "PREVENTIVE_SIMULTANEITY_VIOLATION");
            }
        }

        var now = TimeHelper.NowCR();
        string initialStatus = r.EstimatedStartDate <= now ? MaintenanceStates.InProgress : MaintenanceStates.Scheduled;

        var maintenance = new Maintenance
        {
            TurbineId = r.TurbineId,
            MaintenanceType = r.MaintenanceType,
            EstimatedStartDate = r.EstimatedStartDate,
            EstimatedEndDate = r.EstimatedEndDate,
            Status = initialStatus,
            Created = now
        };

        _maintenanceCrudFactory.Create(maintenance);

        // Si inicia de inmediato o es el flujo estándar, pasar la turbina a UnderMaintenance
        if (initialStatus == MaintenanceStates.InProgress || turbine.Status == TurbineStates.Active || turbine.Status == TurbineStates.Damaged || turbine.Status == TurbineStates.SuspendedForNonCompliance)
        {
            new TurbineManager().ChangeState(new ChangeTurbineStateRequest
            {
                TurbineId = r.TurbineId,
                NewState = TurbineStates.UnderMaintenance,
                Reason = $"Registered {r.MaintenanceType} maintenance"
            }, callerUserId);
        }

        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.Maintenances, AuditActions.Create, "tblMaintenances", r.TurbineId, null, $"Registered {r.MaintenanceType} maintenance on turbine {turbine.UniqueCode}");
    }

    // RF-017: Completa un mantenimiento en curso. Actualiza fecha de último mantenimiento y retorna la turbina a Active.
    public void Complete(CompleteMaintenanceRequest r, int callerUserId)
    {
        var maintenance = _maintenanceCrudFactory.RetrieveById<Maintenance>(r.MaintenanceId) ?? throw new NotFoundException("Maintenance not found.");

        if (string.Equals(maintenance.Status, MaintenanceStates.Completed, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(maintenance.Status, MaintenanceStates.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Maintenance is already closed.", "MAINTENANCE_CLOSED");
        }

        var now = TimeHelper.NowCR();
        _maintenanceCrudFactory.Complete(r.MaintenanceId, now, r.Result, now);

        _turbineCrudFactory.UpdateMaintenanceDate(maintenance.TurbineId, now, now);

        new TurbineManager().ChangeState(new ChangeTurbineStateRequest
        {
            TurbineId = maintenance.TurbineId,
            NewState = TurbineStates.Active,
            Reason = $"Maintenance {maintenance.Id} completed: {r.Result}"
        }, callerUserId);

        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.Maintenances, AuditActions.Update, "tblMaintenances", maintenance.Id, maintenance.Status, MaintenanceStates.Completed);
    }

    // Cancela un mantenimiento programado. Solo permitido si el estado es Scheduled.
    public void Cancel(int maintenanceId, int callerUserId)
    {
        var maintenance = _maintenanceCrudFactory.RetrieveById<Maintenance>(maintenanceId) ?? throw new NotFoundException("Maintenance not found.");

        if (!string.Equals(maintenance.Status, MaintenanceStates.Scheduled, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Only scheduled maintenances can be cancelled.", "INVALID_MAINTENANCE_STATUS");
        }

        var now = TimeHelper.NowCR();
        maintenance.Status = MaintenanceStates.Cancelled;
        maintenance.Updated = now;

        _maintenanceCrudFactory.Update(maintenance);

        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.Maintenances, AuditActions.Update, "tblMaintenances", maintenance.Id, MaintenanceStates.Scheduled, MaintenanceStates.Cancelled);
    }

    // Retorna el historial de mantenimientos de una turbina específica.
    public List<Maintenance> RetrieveByTurbine(int turbineId)
    {
        return _maintenanceCrudFactory.RetrieveByTurbine(turbineId);
    }

    // RF-019 / RN-010: Verifica si existe algún mantenimiento preventivo activo en la red.
    public Maintenance? CheckActivePreventive()
    {
        return _maintenanceCrudFactory.RetrieveActivePreventive();
    }

    // Retorna todos los mantenimientos registrados en el sistema.
    public List<Maintenance> RetrieveAll()
    {
        return _maintenanceCrudFactory.RetrieveAll<Maintenance>();
    }

    private static void ValidateMaintenanceInput(string? maintenanceType, DateTime estimatedStartDate, DateTime estimatedEndDate)
    {
        if (string.IsNullOrWhiteSpace(maintenanceType))
            throw new BusinessException("MaintenanceType is required.", "INVALID_MAINTENANCE_TYPE");

        if (!maintenanceType.Equals(MaintenanceTypes.Preventive, StringComparison.OrdinalIgnoreCase) &&
            !maintenanceType.Equals(MaintenanceTypes.Corrective, StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("MaintenanceType must be 'Preventive' or 'Corrective'.", "INVALID_MAINTENANCE_TYPE");

        if (estimatedStartDate >= estimatedEndDate)
            throw new BusinessException("EstimatedStartDate must be before EstimatedEndDate.", "INVALID_MAINTENANCE_DATES");
    }
}
