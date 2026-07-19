using SEGEDE_Grupo1.DataAccess.CRUD;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

namespace SEGEDE_Grupo1.CoreApp;

// Manager de Banco Central (§14.7). Instanciación directa con new sin IoC.
// Gestiona la consulta del singleton del banco central (inventario y capacidad efectiva), configuración de capacidad manual e historial de movimientos.
public class CentralBankManager
{
    private readonly CentralBankCrudFactory _cbFactory = new();
    private readonly CentralBankLogCrudFactory _logFactory = new();
    private readonly AuditManager _auditManager = new();

    // RF-038: Retorna el estado actual del Banco Central (inventario actual, capacidad manual, automática y efectiva).
    public CentralBank Retrieve()
    {
        var cb = _cbFactory.RetrieveSingleton();
        if (cb == null)
        {
            throw new NotFoundException("Central Bank singleton record not found.");
        }
        return cb;
    }

    // RF-039: Permite al Administrador establecer o limpiar la capacidad manual del Banco Central.
    // Un valor null limpia la capacidad manual, haciendo que la capacidad efectiva sea igual a la automática.
    public void SetManualCapacity(SetManualCapacityRequest r, int callerUserId)
    {
        if (r.Capacity.HasValue && r.Capacity.Value < 0)
        {
            throw new BusinessException("Manual capacity cannot be negative.", "INVALID_CAPACITY");
        }

        var existing = Retrieve();
        string oldVal = existing.ManualCapacity.HasValue ? existing.ManualCapacity.Value.ToString() : "null";
        string newVal = r.Capacity.HasValue ? r.Capacity.Value.ToString() : "null";

        _cbFactory.UpdateManualCapacity(r.Capacity, TimeHelper.NowCR());

        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.CentralBank, AuditActions.Update, "tblCentralBank", 1, oldVal, newVal);
    }

    // RF-043: Retorna el historial paginado de movimientos de inventario del Banco Central (inflows y outflows).
    public PagedResponse<CentralBankLog> RetrieveLogs(PagedRequest p)
    {
        var all = _logFactory.RetrieveAll<CentralBankLog>();
        var items = all.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToList();
        int totalPages = all.Count == 0 ? 0 : (int)Math.Ceiling(all.Count / (double)p.PageSize);

        return new PagedResponse<CentralBankLog>
        {
            Items = items,
            Page = p.Page,
            PageSize = p.PageSize,
            TotalCount = all.Count,
            TotalPages = totalPages
        };
    }
}
