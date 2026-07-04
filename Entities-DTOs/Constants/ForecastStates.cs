namespace SEGEDE_Grupo1.EntitiesDTOs.Constants;

/// <summary>
/// Estados de pronóstico de demanda (§4).
/// </summary>
public static class ForecastStates
{
    public const string Pending = "Pending";
    public const string Modified = "Modified";
    public const string Blocked = "Blocked";
    public const string Distributed = "Distributed";
    public const string Cancelled = "Cancelled";
}
