namespace SEGEDE_Grupo1.EntitiesDTOs.Constants;

/// <summary>
/// Estados de flush (§4).
/// </summary>
public static class FlushStates
{
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string Failed = "Failed";
}
