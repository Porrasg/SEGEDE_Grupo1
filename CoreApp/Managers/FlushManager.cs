using SEGEDE_Grupo1.DataAccess.CRUD;
using SEGEDE_Grupo1.EntitiesDTOs.Constants;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;
using SEGEDE_Grupo1.EntitiesDTOs.Exceptions;
using SEGEDE_Grupo1.EntitiesDTOs.Helpers;

namespace SEGEDE_Grupo1.CoreApp.Managers;

/// <summary>
/// Manager de Vaciado (Flush) (§14.6). Instanciación directa con new sin IoC.
/// Ejecuta ciclos de vaciado automático y manual de las baterías locales hacia el Banco Central bajo semántica ACID (§17.2), manejando saturación e idempotencia.
/// </summary>
public class FlushManager
{
    private readonly FlushCrudFactory _flushFactory = new();
    private readonly FlushConfigCrudFactory _configFactory = new();
    private readonly FlushSnapshotCrudFactory _snapshotFactory = new();
    private readonly SaturationLogCrudFactory _satLogFactory = new();
    private readonly LocalBatteryCrudFactory _batteryFactory = new();
    private readonly CentralBankCrudFactory _cbFactory = new();
    private readonly CentralBankLogCrudFactory _cbLogFactory = new();
    private readonly AuditManager _auditManager = new();

    /// <summary>
    /// RF-031/033-037 (§17.2): Ejecuta el vaciado automático de baterías al Banco Central. Actor del sistema.
    /// </summary>
    public void ExecuteAutoFlush()
    {
        PerformFlush(FlushTypes.Automatic, null);
    }

    /// <summary>
    /// RF-032: Ejecuta un vaciado manual solicitado por un usuario autorizado. Valida que no haya flush activo y que exista energía para vaciar (RN-019).
    /// </summary>
    public void ExecuteManualFlush(int callerUserId)
    {
        PerformFlush(FlushTypes.Manual, callerUserId);
    }

    /// <summary>
    /// RF-031: Retorna la configuración de flush (hora de ejecución y si está activo el modo automático).
    /// </summary>
    public FlushConfig GetFlushConfig()
    {
        var cfg = _configFactory.RetrieveSingleton();
        if (cfg == null)
        {
            throw new NotFoundException("Flush configuration singleton not found.");
        }
        return cfg;
    }

    /// <summary>
    /// RF-031: Actualiza la configuración de flush (requiere rol de Administrador).
    /// </summary>
    public void UpdateFlushConfig(UpdateFlushConfigRequest r, int callerUserId)
    {
        var existing = GetFlushConfig();
        string oldVal = $"{existing.ExecutionTime}|{existing.IsAutomatic}";
        string newVal = $"{r.ExecutionTime}|{r.IsAutomatic}";

        _configFactory.UpdateSingleton(r.ExecutionTime, r.IsAutomatic, TimeHelper.NowCR());

        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.CentralBank, AuditActions.Update, "tblFlushConfig", 1, oldVal, newVal);
    }

    /// <summary>
    /// Retorna el historial paginado de operaciones de flush.
    /// </summary>
    public PagedResponse<Flush> RetrieveFlushHistory(PagedRequest p)
    {
        var all = _flushFactory.RetrieveAll<Flush>();
        var items = all.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToList();
        int totalPages = all.Count == 0 ? 0 : (int)Math.Ceiling(all.Count / (double)p.PageSize);

        return new PagedResponse<Flush>
        {
            Items = items,
            Page = p.Page,
            PageSize = p.PageSize,
            TotalCount = all.Count,
            TotalPages = totalPages
        };
    }

    /// <summary>
    /// RF-035: Verifica si existe un flush activo en estado Processing.
    /// </summary>
    public Flush? CheckActiveFlush()
    {
        return _flushFactory.RetrieveActive();
    }

    // --- Helper Privado de Vaciado ACID (§17.2) ---

    private void PerformFlush(string executionType, int? userId)
    {
        var active = CheckActiveFlush();
        if (active != null)
        {
            throw new BusinessException("A flush operation is already currently in progress.", "FLUSH_IN_PROGRESS");
        }

        var nonEmptyBatteries = _batteryFactory.RetrieveAllNonEmpty();
        decimal totalAvailableEnergy = nonEmptyBatteries.Sum(b => b.StoredEnergy);

        if (executionType == FlushTypes.Manual && totalAvailableEnergy <= 0)
        {
            throw new BusinessException("No energy to flush in any local battery (RN-019).", "NO_ENERGY_TO_FLUSH");
        }

        if (totalAvailableEnergy <= 0 && nonEmptyBatteries.Count == 0)
        {
            return; // En automático, si no hay energía, no se genera transacción en vacío
        }

        var now = TimeHelper.NowCR();
        var flush = new Flush
        {
            ExecutionType = executionType,
            Status = FlushStates.Processing,
            UserId = userId,
            TotalTransferredEnergy = 0m,
            SaturationLoss = 0m,
            StartDate = now,
            EndDate = null,
            Created = now
        };

        _flushFactory.Create(flush);
        var createdFlush = _flushFactory.RetrieveActive() ?? throw new BusinessException("Failed to initiate flush transaction.");

        try
        {
            decimal totalSnapshotEnergy = 0m;
            var snapshots = new List<FlushSnapshot>();

            foreach (var b in nonEmptyBatteries)
            {
                if (b.StoredEnergy <= 0) continue;

                var snap = new FlushSnapshot
                {
                    FlushId = createdFlush.Id,
                    TurbineId = b.TurbineId,
                    LocalBatteryId = b.Id,
                    CapturedEnergy = b.StoredEnergy,
                    EventDate = now,
                    Created = now
                };
                _snapshotFactory.Create(snap);
                snapshots.Add(snap);
                totalSnapshotEnergy += b.StoredEnergy;
            }

            var cb = _cbFactory.RetrieveSingleton() ?? throw new NotFoundException("Central Bank not found.");
            decimal ia = cb.CurrentInventory;
            decimal et = totalSnapshotEnergy;
            decimal cmbc = cb.EffectiveCapacity;

            decimal finalInv;
            decimal ps = 0m;
            decimal transferred = et;

            if (ia + et > cmbc)
            {
                ps = (ia + et) - cmbc;
                finalInv = cmbc;
                transferred = et - ps;

                var satLog = new SaturationLog
                {
                    FlushId = createdFlush.Id,
                    PreviousInventory = ia,
                    NewInventory = finalInv,
                    ExcessEnergy = ps,
                    EventDate = now,
                    Created = now
                };
                _satLogFactory.Create(satLog);
            }
            else
            {
                finalInv = ia + et;
            }

            _cbFactory.UpdateInventory(finalInv, now);

            if (transferred > 0)
            {
                var cbLog = new CentralBankLog
                {
                    MovementType = MovementTypes.Inflow,
                    Amount = transferred,
                    ResultingInventory = finalInv,
                    FlushId = createdFlush.Id,
                    DistributionId = null,
                    EventDate = now,
                    Created = now
                };
                _cbLogFactory.Create(cbLog);
            }

            foreach (var snap in snapshots)
            {
                _batteryFactory.UpdateEnergy(snap.LocalBatteryId, 0m, now);
            }

            _flushFactory.UpdateStatus(createdFlush.Id, FlushStates.Completed, now, transferred, ps, now);

            _auditManager.LogAction(userId, userId.HasValue ? $"User {userId}" : SystemActor.Name, AuditModules.CentralBank, AuditActions.Execute, "tblFlush", createdFlush.Id, FlushStates.Processing, FlushStates.Completed);
        }
        catch (Exception ex)
        {
            _flushFactory.UpdateStatus(createdFlush.Id, FlushStates.Failed, TimeHelper.NowCR(), 0m, 0m, TimeHelper.NowCR());
            _auditManager.LogAction(userId, userId.HasValue ? $"User {userId}" : SystemActor.Name, AuditModules.CentralBank, AuditActions.Execute, "tblFlush", createdFlush.Id, FlushStates.Processing, FlushStates.Failed);
            throw new BusinessException($"Flush execution failed: {ex.Message}", "FLUSH_FAILED");
        }
    }
}
