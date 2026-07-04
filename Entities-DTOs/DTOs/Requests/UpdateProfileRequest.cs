namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>
/// Actualización de perfil por Buyer (RF-010, §8.1).
/// </summary>
public class UpdateProfileRequest
{
    public string Phone { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string? NewPassword { get; set; }
}
