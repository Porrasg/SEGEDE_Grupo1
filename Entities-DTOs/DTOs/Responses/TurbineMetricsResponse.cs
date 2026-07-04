namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;

/// <summary>Métricas operacionales de una turbina (§8.2).</summary>
public class TurbineMetricsResponse
{
    public int TurbineId { get; set; }
    public decimal TotalActiveSeconds { get; set; }
    public decimal TotalInactiveSeconds { get; set; }
    public decimal TotalSeconds { get; set; }
    public decimal OperationalAvailability { get; set; }
    public decimal OperationalUnavailability { get; set; }
    public int TotalFailures { get; set; }
    public int TotalMaintenances { get; set; }
    public decimal MTBF { get; set; }
    public decimal MTTR { get; set; }
}
