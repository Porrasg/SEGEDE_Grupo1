namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Detalle de distribución por comprador → tblDistributionDetail (§9.18).
/// </summary>
public class DistributionDetail : BaseDTO
{
    public int DistributionId { get; set; }
    public int BuyerId { get; set; }
    public int ForecastId { get; set; }
    public decimal RequestedMWh { get; set; }
    public decimal AssignedMWh { get; set; }
    public decimal UnsuppliedDemand { get; set; }
}
