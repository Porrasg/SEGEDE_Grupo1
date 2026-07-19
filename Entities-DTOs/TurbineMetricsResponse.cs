namespace SEGEDE_Grupo1.EntitiesDTOs;

// Métricas operacionales de una turbina (§8.2).
public class TurbineMetricsResponse
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int TurbineId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal TotalActiveSeconds { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal TotalInactiveSeconds { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal TotalSeconds { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal OperationalAvailability { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal OperationalUnavailability { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int TotalFailures { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int TotalMaintenances { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal MTBF { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal MTTR { get; set; }
}
