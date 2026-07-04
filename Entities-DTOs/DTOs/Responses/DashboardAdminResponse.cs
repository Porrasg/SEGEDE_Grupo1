namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;

/// <summary>Dashboard de administrador (§8.2).</summary>
public class DashboardAdminResponse
{
    public int TotalTurbines { get; set; }
    public int ActiveTurbines { get; set; }
    public decimal CentralBankInventory { get; set; }
    public decimal EffectiveCapacity { get; set; }
    public int MonthForecasts { get; set; }
    public decimal MonthTotalDemand { get; set; }
    public decimal MonthTotalBilled { get; set; }
    public DateTime? LastFlush { get; set; }
}
