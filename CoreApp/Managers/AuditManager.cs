using System.Text.Json;
using SEGEDE_Grupo1.DataAccess.CRUD;
using SEGEDE_Grupo1.EntitiesDTOs.Constants;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;
using SEGEDE_Grupo1.EntitiesDTOs.Exceptions;
using SEGEDE_Grupo1.EntitiesDTOs.Helpers;

namespace SEGEDE_Grupo1.CoreApp.Managers;

/// <summary>
/// Manager de Auditoría (§14.12). Gestiona el registro WORM de todas las acciones
/// del sistema, archivado en frío y consultas con filtro de seguridad RN-030.
/// Instanciación de fábricas directa con new sin IoC.
/// </summary>
public class AuditManager
{
    private readonly AuditLogCrudFactory _auditCrudFactory = new();

    /// <summary>
    /// Registra una acción en la pista de auditoría (§17.9).
    /// </summary>
    /// <param name="userId">ID del usuario (null si el actor es System).</param>
    /// <param name="userName">Nombre del usuario o "System".</param>
    /// <param name="module">Módulo afectado (ver AuditModules).</param>
    /// <param name="action">Acción realizada (ver AuditActions).</param>
    /// <param name="entity">Nombre de la entidad modificada.</param>
    /// <param name="entityId">ID de la entidad modificada.</param>
    /// <param name="previousValue">Objeto o cadena con el valor anterior (serializado a JSON).</param>
    /// <param name="newValue">Objeto o cadena con el nuevo valor (serializado a JSON).</param>
    public void LogAction(int? userId, string userName, string module, string action, string entity, int entityId, object? previousValue, object? newValue)
    {
        string? prevJson = previousValue is string sPrev ? sPrev : (previousValue != null ? JsonSerializer.Serialize(previousValue) : null);
        string? newJson = newValue is string sNew ? sNew : (newValue != null ? JsonSerializer.Serialize(newValue) : null);

        var log = new AuditLog
        {
            UserId = userId,
            UserName = string.IsNullOrWhiteSpace(userName) ? "System" : userName,
            Module = module,
            Action = action,
            AffectedEntity = entity,
            EntityId = entityId,
            PreviousValue = prevJson,
            NewValue = newJson,
            EventDate = TimeHelper.NowCR(),
            IsColdArchive = false,
            Created = TimeHelper.NowCR()
        };

        _auditCrudFactory.Create(log);
    }

    /// <summary>
    /// Retorna registros de auditoría filtrados por módulo con paginación (§14.12).
    /// Aplica regla RN-030: los ingenieros no pueden ver auditorías del módulo Billing.
    /// </summary>
    public PagedResponse<AuditLog> RetrieveByModule(string module, string callerRole, PagedRequest p)
    {
        if (string.Equals(callerRole, "Engineer", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(module, AuditModules.Billing, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessAppException("Engineers are not authorized to access Billing audit logs.", "UNAUTHORIZED_AUDIT_ACCESS");
        }

        var items = _auditCrudFactory.RetrieveByModule(module, p.Page, p.PageSize);
        return BuildPagedResponse(items, p);
    }

    /// <summary>
    /// Retorna registros de auditoría filtrados por usuario con paginación (§14.12).
    /// </summary>
    public PagedResponse<AuditLog> RetrieveByUser(int userId, PagedRequest p)
    {
        var items = _auditCrudFactory.RetrieveByUser(userId, p.Page, p.PageSize);
        return BuildPagedResponse(items, p);
    }

    /// <summary>
    /// Retorna registros de auditoría en un rango de fechas (§14.12).
    /// Excluye registros del módulo Billing si el rol es Engineer (RN-030).
    /// </summary>
    public PagedResponse<AuditLog> RetrieveByDateRange(DateTime from, DateTime to, string callerRole, PagedRequest p)
    {
        var items = _auditCrudFactory.RetrieveByDateRange(from, to, p.Page, p.PageSize);

        if (string.Equals(callerRole, "Engineer", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(x => !string.Equals(x.Module, AuditModules.Billing, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return BuildPagedResponse(items, p);
    }

    /// <summary>
    /// Job anual que marca registros con más de 5 años como IsColdArchive=true (§14.12 / §17.4).
    /// </summary>
    public void ArchiveColdRecords()
    {
        var threshold = TimeHelper.NowCR().AddYears(-5);
        _auditCrudFactory.MarkColdArchive(threshold);
        LogAction(null, "System", AuditModules.System, AuditActions.Update, "tblAuditLog", 0, null, $"Cold archive threshold: {threshold:yyyy-MM-dd}");
    }

    private static PagedResponse<T> BuildPagedResponse<T>(List<T> items, PagedRequest p)
    {
        int totalCount = items.Count;
        return new PagedResponse<T>
        {
            Items = items,
            Page = p.Page,
            PageSize = p.PageSize,
            TotalCount = totalCount,
            TotalPages = totalCount > 0 ? (int)Math.Ceiling((double)totalCount / p.PageSize) : 1
        };
    }
}
