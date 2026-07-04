namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Log de generación de energía → tblEnergyGenerationLog (§9.8). Insert-only.
/// </summary>
public class EnergyGenerationLog : BaseDTO
{
    public int TurbineId { get; set; }
    public decimal ActiveTimeSeconds { get; set; }
    public decimal GeneratedEnergy { get; set; }
    public DateTime EventDate { get; set; }
}
