namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>
/// Completar mantenimiento (RF-017, §8.1).
/// </summary>
public class CompleteMaintenanceRequest
{
    public int MaintenanceId { get; set; }
    public string Result { get; set; } = string.Empty;
}
