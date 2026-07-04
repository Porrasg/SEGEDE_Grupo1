namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Log de auditoría → tblAuditLog (§9.23). WORM.
/// </summary>
public class AuditLog : BaseDTO
{
    public int? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string AffectedEntity { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? PreviousValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime EventDate { get; set; }
    public bool IsColdArchive { get; set; }
}
