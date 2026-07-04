namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Falla de turbina → tblFailures (§9.7).
/// </summary>
public class Failure : BaseDTO
{
    public int TurbineId { get; set; }
    public DateTime FailureDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}
