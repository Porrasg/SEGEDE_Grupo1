namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

// Configuración de flush automático (RF-031, §8.1).
public class UpdateFlushConfigRequest
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public TimeSpan ExecutionTime { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public bool IsAutomatic { get; set; }
}
