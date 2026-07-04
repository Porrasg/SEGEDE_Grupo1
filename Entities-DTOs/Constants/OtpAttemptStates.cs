namespace SEGEDE_Grupo1.EntitiesDTOs.Constants;

// Estados de intentos OTP (§4).
public static class OtpAttemptStates
{
    public const string InProgress = "InProgress";
    public const string Verified = "Verified";
    public const string Blocked = "Blocked";
}
