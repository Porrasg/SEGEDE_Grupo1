namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>Regeneración de estado de cuenta (RF-063, §8.1).</summary>
public class RegenerateStatementRequest
{
    public int OriginalStatementId { get; set; }
}
