namespace SEGEDE_Grupo1.EntitiesDTOs;

// Estados de flush (§4).
public static class FlushStates
{
    public const string Processing = "InProgress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string Failed = "Failed";
}
