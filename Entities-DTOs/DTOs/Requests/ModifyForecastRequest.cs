namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>
/// Modificación de pronóstico (RF-046, §8.1).
/// </summary>
public class ModifyForecastRequest
{
    public int ForecastId { get; set; }
    public decimal NewAmountMWh { get; set; }
}
