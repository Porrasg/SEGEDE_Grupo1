using System.Data;
using Microsoft.Data.SqlClient;
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

        // Crear el registro de Flush FUERA de la transacción principal para poder marcarlo Failed si el
        // trabajo transaccional falla (la fila sobrevive al rollback). El trabajo crítico (snapshots,
        // inventario y baterías) se ejecuta dentro de una única transacción Serializable para garantizar ACID.
        //
        // Garantía de concurrencia: el índice único filtrado UX_Flush_Active sobre tblFlush(Status)
        // WHERE Status='Processing' (ver Database/03_Index_Flush_Active.sql) impide a NIVEL DE BD que
        // exista más de un flush 'Processing' a la vez. Si dos procesos intentan crear en paralelo, el
        // segundo INSERT viola el índice y se traduce a FLUSH_IN_PROGRESS (409) — protección atómica y
        // real, no dependiente del re-chequeo por Id (que queda como defensa adicional si el índice no
        // estuviera desplegado en una BD dada).
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

        try
        {
            _flushFactory.Create(flush);
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            // Violación del índice único filtrado: ya existe un flush 'Processing' concurrente.
            throw new BusinessException("A flush operation is already currently in progress.", "FLUSH_IN_PROGRESS");
        }

        var createdFlush = _flushFactory.RetrieveActive() ?? throw new BusinessException("Failed to initiate flush transaction.");

        // Ejecutar las operaciones críticas en una única transacción Serializable.
        using var conn = SqlDao.GetInstance().GetOpenConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            // Re-chequeo dentro de la transacción: si existe otro flush activo distinto al que acabamos de crear,
            // abortamos. Si la única fila "Processing" es la que creamos arriba (misma Id), continuamos.
            var alreadyActive = _flushFactory.RetrieveActive(conn, tx);
            if (alreadyActive != null && alreadyActive.Id != createdFlush.Id)
            {
                throw new BusinessException("A flush operation is already currently in progress.", "FLUSH_IN_PROGRESS");
            }

            decimal totalSnapshotEnergy = 0m;
            var snapshots = new List<FlushSnapshot>();

            // Leer baterías dentro de la transacción para evitar carreras.
            var batteriesInTx = _batteryFactory.RetrieveAllNonEmpty(conn, tx);

            foreach (var b in batteriesInTx)
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

            // Marcar el flush como completado dentro de la misma transacción
            _flushFactory.UpdateStatus(createdFlush.Id, FlushStates.Completed, now, transferred, ps, now, conn, tx);

            tx.Commit();

            _auditManager.LogAction(userId, userId.HasValue ? $"User {userId}" : SystemActor.Name, AuditModules.Flush, AuditActions.Execute, "tblFlush", createdFlush.Id, FlushStates.Processing, FlushStates.Completed);
        }
        catch (BusinessException)
        {
            try { tx.Rollback(); } catch { /* la conexión pudo cerrarse antes del rollback */ }
            // Podemos marcar el flush como fallido puesto que fue creado fuera de la transacción principal.
            try
            {
                _flushFactory.UpdateStatus(createdFlush.Id, FlushStates.Failed, TimeHelper.NowCR(), 0m, 0m, TimeHelper.NowCR());
            }
            catch { /* si falla el update de estado, no podemos hacer más aquí */ }
            throw;
        }
        catch (Exception ex)
        {
            try { tx.Rollback(); } catch { /* la conexión pudo cerrarse antes del rollback */ }
            try
            {
                _flushFactory.UpdateStatus(createdFlush.Id, FlushStates.Failed, TimeHelper.NowCR(), 0m, 0m, TimeHelper.NowCR());
            }
            catch { /* ignorable */ }
            _auditManager.LogAction(userId, userId.HasValue ? $"User {userId}" : SystemActor.Name, AuditModules.Flush, AuditActions.Execute, "tblFlush", createdFlush.Id, FlushStates.Processing, FlushStates.Failed);
            throw new BusinessException($"Flush execution failed: {ex.Message}", "FLUSH_FAILED");
        }
    }
}
