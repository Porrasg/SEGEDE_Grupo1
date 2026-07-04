namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;

/// <summary>Dashboard de comprador (§8.2).</summary>
public class DashboardBuyerResponse
{
    public int ActiveForecasts { get; set; }
    public decimal MonthRequestedMWh { get; set; }
    public decimal LastAssignment { get; set; }
    public decimal TotalBilledAccumulated { get; set; }
    public DateTime? LastStatementDate { get; set; }
}
