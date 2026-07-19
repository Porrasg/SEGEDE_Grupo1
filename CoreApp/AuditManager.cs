using System.Text.Json;
using SEGEDE_Grupo1.DataAccess.CRUD;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.CoreApp.Helpers;
using SEGEDE_Grupo1.CoreApp.Exceptions;

namespace SEGEDE_Grupo1.CoreApp;

// Manager de Auditoría (§14.12). Gestiona el registro WORM de todas las acciones
// del sistema, archivado en frío y consultas con filtro de seguridad RN-030.
// Instanciación de fábricas directa con new sin IoC.
public class AuditManager
{
    private readonly AuditLogCrudFactory _auditCrudFactory = new();

    // Registra una acción en la pista de auditoría (§17.9).
    // Parámetro userId: ID del usuario (null si el actor es System).
    // Parámetro userName: Nombre del usuario o "System".
    // Parámetro module: Módulo afectado (ver AuditModules).
    // Parámetro action: Acción realizada (ver AuditActions).
    // Parámetro entity: Nombre de la entidad modificada.
    // Parámetro entityId: ID de la entidad modificada.
    // Parámetro previousValue: Objeto o cadena con el valor anterior (serializado a JSON).
    // Parámetro newValue: Objeto o cadena con el nuevo valor (serializado a JSON).
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

    // Retorna registros de auditoría filtrados por módulo con paginación (§14.12).
    // Aplica regla RN-030: los ingenieros no pueden ver auditorías del módulo Billing.
    public List<AuditLog> RetrieveByModule(string module, string callerRole)
    {
        if (string.Equals(callerRole, "Engineer", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(module, AuditModules.Billing, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessAppException("Engineers are not authorized to access Billing audit logs.", "UNAUTHORIZED_AUDIT_ACCESS");
        }

        return _auditCrudFactory.RetrieveByModule(module, 1, 10000);
    }

    // Retorna registros de auditoría filtrados por usuario (§14.12).
    public List<AuditLog> RetrieveByUser(int userId)
    {
        return _auditCrudFactory.RetrieveByUser(userId, 1, 10000);
    }

    // Retorna registros de auditoría en un rango de fechas (§14.12).
    // Excluye registros del módulo Billing si el rol es Engineer (RN-030).
    public List<AuditLog> RetrieveByDateRange(DateTime from, DateTime to, string callerRole)
    {
        var items = _auditCrudFactory.RetrieveByDateRange(from, to, 1, 10000);

        if (string.Equals(callerRole, "Engineer", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(x => !string.Equals(x.Module, AuditModules.Billing, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return items;
    }

    // Job anual que marca registros con más de 5 años como IsColdArchive=true (§14.12 / §17.4).
    public void ArchiveColdRecords()
    {
        var threshold = TimeHelper.NowCR().AddYears(-5);
        _auditCrudFactory.MarkColdArchive(threshold);
        LogAction(null, "System", AuditModules.System, AuditActions.Update, "tblAuditLog", 0, null, $"Cold archive threshold: {threshold:yyyy-MM-dd}");
    }

    }
