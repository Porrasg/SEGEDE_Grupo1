namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>
/// Reenvío de OTP (RF-007, §8.1).
/// </summary>
public class ResendOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public string UsageType { get; set; } = string.Empty;
}
