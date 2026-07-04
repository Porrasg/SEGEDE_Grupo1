namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>
/// Registro de falla (RF-020, §8.1).
/// </summary>
public class RegisterFailureRequest
{
    public int TurbineId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}
