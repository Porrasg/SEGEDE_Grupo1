namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>
/// Cambio de estado de turbina (RF-016, §8.1).
/// </summary>
public class ChangeTurbineStateRequest
{
    public int TurbineId { get; set; }
    public string NewState { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
