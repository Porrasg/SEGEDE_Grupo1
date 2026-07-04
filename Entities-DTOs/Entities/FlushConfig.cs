namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Configuración de flush → tblFlushConfig (§9.10). Singleton (Id=1).
/// </summary>
public class FlushConfig : BaseDTO
{
    public TimeSpan ExecutionTime { get; set; }
    public bool IsAutomatic { get; set; }
}
