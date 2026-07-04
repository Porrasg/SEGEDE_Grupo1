namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>
/// Actualización de turbina (RF-014, §8.1).
/// </summary>
public class UpdateTurbineRequest
{
    public int TurbineId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public decimal WeeklyNominalCapacity { get; set; }
}
