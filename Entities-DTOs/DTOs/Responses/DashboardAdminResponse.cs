namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;

// Dashboard de administrador (§8.2).
public class DashboardAdminResponse
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int TotalTurbines { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int ActiveTurbines { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal CentralBankInventory { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal EffectiveCapacity { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int MonthForecasts { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal MonthTotalDemand { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal MonthTotalBilled { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime? LastFlush { get; set; }
}
