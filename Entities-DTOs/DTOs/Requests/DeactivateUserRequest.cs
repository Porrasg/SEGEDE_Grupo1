namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>
/// Desactivación de usuario (RF-011, §8.1).
/// </summary>
public class DeactivateUserRequest
{
    public int UserId { get; set; }
    public bool CancelForecasts { get; set; }
}
