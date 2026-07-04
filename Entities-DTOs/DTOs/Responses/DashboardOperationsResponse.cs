namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;

// Dashboard de operaciones / ingeniero (§8.2). Nunca expone datos financieros (RN-030).
public class DashboardOperationsResponse
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int TotalTurbines { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int ActiveTurbines { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int TurbinesUnderMaintenance { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int DamagedTurbines { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int SuspendedTurbines { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal CentralBankInventory { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime? LastFlushDate { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal LastFlushEnergy { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int OverdueMaintenanceAlerts { get; set; }
}
