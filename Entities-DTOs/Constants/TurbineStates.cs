namespace SEGEDE_Grupo1.EntitiesDTOs.Constants;

/// <summary>
/// Estados de turbinas (§4).
/// </summary>
public static class TurbineStates
{
    public const string Active = "Active";
    public const string UnderMaintenance = "UnderMaintenance";
    public const string Damaged = "Damaged";
    public const string SuspendedForNonCompliance = "SuspendedForNonCompliance";
    public const string Decommissioned = "Decommissioned";
}
