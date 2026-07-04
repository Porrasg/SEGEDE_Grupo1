namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>
/// Login paso 2 — verificación OTP (RF-005, §8.1).
/// </summary>
public class LoginStep2Request
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}
