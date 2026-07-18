namespace SEGEDE_Grupo1.EntitiesDTOs;

// Anulación de estado de cuenta (RF-062, §8.1).
public class AnnulStatementRequest
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int StatementId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Reason { get; set; } = string.Empty;
}
