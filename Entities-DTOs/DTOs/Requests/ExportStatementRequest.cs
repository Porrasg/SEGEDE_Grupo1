namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>Exportación de estado de cuenta (RF-065, §8.1).</summary>
public class ExportStatementRequest
{
    public int StatementId { get; set; }
    public string Format { get; set; } = string.Empty;
}
