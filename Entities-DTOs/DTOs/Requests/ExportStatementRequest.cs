namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

// Exportación de estado de cuenta (RF-065, §8.1).
public class ExportStatementRequest
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int StatementId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Format { get; set; } = string.Empty;
}
