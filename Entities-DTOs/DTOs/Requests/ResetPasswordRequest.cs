namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>
/// Reset de contraseña con OTP (RF-006, §8.1).
/// </summary>
public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
