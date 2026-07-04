namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>
/// Login paso 1 — email + password (RF-005, §8.1).
/// </summary>
public class LoginStep1Request
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
