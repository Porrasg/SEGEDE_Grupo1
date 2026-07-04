namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Pronóstico de demanda → tblForecast (§9.16).
/// UNIQUE (BuyerId, Month, Year) — solo un forecast activo por combinación.
/// </summary>
public class Forecast : BaseDTO
{
    public int BuyerId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal AmountMWh { get; set; }
    public string Status { get; set; } = string.Empty;
}
