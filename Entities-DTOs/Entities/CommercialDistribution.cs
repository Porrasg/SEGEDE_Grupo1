namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Distribución comercial → tblCommercialDistribution (§9.17).
/// UNIQUE (Month, Year) — una distribución por mes.
/// </summary>
public class CommercialDistribution : BaseDTO
{
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime ExecutionDate { get; set; }
    public decimal AvailableInventory { get; set; }
    public decimal TotalDemand { get; set; }
    public decimal DistributedEnergy { get; set; }
    public decimal RoundingResidual { get; set; }
    public string Scenario { get; set; } = string.Empty;
}
