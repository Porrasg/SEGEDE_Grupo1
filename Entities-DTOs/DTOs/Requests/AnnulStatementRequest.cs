namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>Anulación de estado de cuenta (RF-062, §8.1).</summary>
public class AnnulStatementRequest
{
    public int StatementId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
