namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>
/// Registro de mantenimiento (RF-017, §8.1).
/// </summary>
public class RegisterMaintenanceRequest
{
    public int TurbineId { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public DateTime EstimatedStartDate { get; set; }
    public DateTime EstimatedEndDate { get; set; }
}
