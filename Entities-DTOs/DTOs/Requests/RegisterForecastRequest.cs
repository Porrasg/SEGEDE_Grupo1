namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>
/// Registro de pronóstico de demanda (RF-044, §8.1).
/// </summary>
public class RegisterForecastRequest
{
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal AmountMWh { get; set; }
}
