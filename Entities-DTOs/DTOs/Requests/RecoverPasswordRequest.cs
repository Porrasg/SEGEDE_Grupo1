namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>
/// Solicitud de recuperación de contraseña (RF-006, §8.1).
/// </summary>
public class RecoverPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}
