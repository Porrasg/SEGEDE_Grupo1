namespace SEGEDE_Grupo1.EntitiesDTOs;

// Configuración de flush → tblFlushConfig (§9.10). Singleton (Id=1).
public class FlushConfig : BaseDTO
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public TimeSpan ExecutionTime { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public bool IsAutomatic { get; set; }
}
