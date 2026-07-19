namespace SEGEDE_Grupo1.EntitiesDTOs;

// Regeneración de estado de cuenta (RF-063, §8.1).
public class RegenerateStatementRequest
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int OriginalStatementId { get; set; }
}
