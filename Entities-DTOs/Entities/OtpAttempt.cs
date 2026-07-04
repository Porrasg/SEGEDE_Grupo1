namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Intento de OTP → tblOtpAttempts (§9.2).
/// </summary>
public class OtpAttempt : BaseDTO
{
    public int UserId { get; set; }
    public string UsageType { get; set; } = string.Empty;
    public int ResendCount { get; set; }
    public int FailedAttempts { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime WindowExpiration { get; set; }
}
