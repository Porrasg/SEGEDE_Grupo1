namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Mantenimiento de turbina → tblMaintenances (§9.6).
/// </summary>
public class Maintenance : BaseDTO
{
    public int TurbineId { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public DateTime EstimatedStartDate { get; set; }
    public DateTime EstimatedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public string? Result { get; set; }
    public string Status { get; set; } = string.Empty;
}
