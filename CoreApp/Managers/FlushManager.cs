using System.Data;
using SEGEDE_Grupo1.DataAccess.CRUD;
using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs.Constants;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;
using SEGEDE_Grupo1.EntitiesDTOs.Exceptions;
using SEGEDE_Grupo1.EntitiesDTOs.Helpers;

namespace SEGEDE_Grupo1.CoreApp.Managers;

// Manager de Vaciado (Flush) (§14.6). Instanciación directa con new sin IoC.
// Ejecuta ciclos de vaciado automático y manual de las baterías locales hacia el Banco Central bajo semántica ACID (§17.2), manejando saturación e idempotencia.
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

    // RF-031/033-037 (§17.2): Ejecuta el vaciado automático de baterías al Banco Central. Actor del sistema.
    public void ExecuteAutoFlush()
    {
        PerformFlush(FlushTypes.Automatic, null);
    }

    // RF-032: Ejecuta un vaciado manual solicitado por un usuario autorizado. Valida que no haya flush activo y que exista energía para vaciar (RN-019).
    public void ExecuteManualFlush(int callerUserId)
    {
        PerformFlush(FlushTypes.Manual, callerUserId);
    }

    // RF-031: Retorna la configuración de flush (hora de ejecución y si está activo el modo automático).
    public FlushConfig GetFlushConfig()
    {
        var cfg = _configFactory.RetrieveSingleton();
        if (cfg == null)
        {
            throw new NotFoundException("Flush configuration singleton not found.");
        }
        return cfg;
    }

    // RF-031: Actualiza la configuración de flush (requiere rol de Administrador).
    public void UpdateFlushConfig(UpdateFlushConfigRequest r, int callerUserId)
    {
        var existing = GetFlushConfig();
        string oldVal = $"{existing.ExecutionTime}|{existing.IsAutomatic}";
        string newVal = $"{r.ExecutionTime}|{r.IsAutomatic}";

        _configFactory.UpdateSingleton(r.ExecutionTime, r.IsAutomatic, TimeHelper.NowCR());

        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.Flush, AuditActions.Update, "tblFlushConfig", 1, oldVal, newVal);
    }

    // Retorna el historial paginado de operaciones de flush.
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

    // RF-035: Verifica si existe un flush activo en estado Processing.
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

        // Ciclo ACID real (§37.25/§60.1): una única transacción con aislamiento Serializable envuelve
        // la creación del Flush, sus snapshots, el ajuste del Banco Central y el vaciado de baterías.
        // Ante cualquier fallo, se revierte todo el lote — no queda ningún registro "Processing" huérfano.
        using var conn = SqlDao.GetInstance().GetOpenConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.Serializable);

        try
        {
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

            _flushFactory.Create(flush, conn, tx);
            var createdFlush = _flushFactory.RetrieveActive(conn, tx) ?? throw new BusinessException("Failed to initiate flush transaction.");

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
                _snapshotFactory.Create(snap, conn, tx);
                snapshots.Add(snap);
                totalSnapshotEnergy += b.StoredEnergy;
            }

            var cb = _cbFactory.RetrieveSingleton(conn, tx) ?? throw new NotFoundException("Central Bank not found.");
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
                _satLogFactory.Create(satLog, conn, tx);
            }
            else
            {
                finalInv = ia + et;
            }

            _cbFactory.UpdateInventory(finalInv, now, conn, tx);

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
                _cbLogFactory.Create(cbLog, conn, tx);
            }

            foreach (var snap in snapshots)
            {
                _batteryFactory.UpdateEnergy(snap.LocalBatteryId, 0m, now, conn, tx);
            }

            _flushFactory.UpdateStatus(createdFlush.Id, FlushStates.Completed, now, transferred, ps, now, conn, tx);

            tx.Commit();

            _auditManager.LogAction(userId, userId.HasValue ? $"User {userId}" : SystemActor.Name, AuditModules.Flush, AuditActions.Execute, "tblFlush", createdFlush.Id, FlushStates.Processing, FlushStates.Completed);
        }
        catch (Exception ex)
        {
            try { tx.Rollback(); } catch { /* la conexión pudo cerrarse antes del rollback */ }
            _auditManager.LogAction(userId, userId.HasValue ? $"User {userId}" : SystemActor.Name, AuditModules.Flush, AuditActions.Execute, "tblFlush", 0, FlushStates.Processing, FlushStates.Failed);
            throw new BusinessException($"Flush execution failed: {ex.Message}", "FLUSH_FAILED");
        }
    }
}
