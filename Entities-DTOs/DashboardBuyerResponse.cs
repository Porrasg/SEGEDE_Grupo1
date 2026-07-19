namespace SEGEDE_Grupo1.EntitiesDTOs;

// Dashboard de comprador (§8.2).
public class DashboardBuyerResponse
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int ActiveForecasts { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal MonthRequestedMWh { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal LastAssignment { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal TotalBilledAccumulated { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime? LastStatementDate { get; set; }
}
