namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;

/// <summary>Dashboard de operaciones / ingeniero (§8.2). Nunca expone datos financieros (RN-030).</summary>
public class DashboardOperationsResponse
{
    public int TotalTurbines { get; set; }
    public int ActiveTurbines { get; set; }
    public int TurbinesUnderMaintenance { get; set; }
    public int DamagedTurbines { get; set; }
    public int SuspendedTurbines { get; set; }
    public decimal CentralBankInventory { get; set; }
    public DateTime? LastFlushDate { get; set; }
    public decimal LastFlushEnergy { get; set; }
    public int OverdueMaintenanceAlerts { get; set; }
}
