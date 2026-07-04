namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Log de pérdida de energía → tblEnergyLossLog (§9.9). Insert-only.
/// </summary>
public class EnergyLossLog : BaseDTO
{
    public int TurbineId { get; set; }
    public decimal InactiveTimeSeconds { get; set; }
    public decimal LostEnergy { get; set; }
    public string Cause { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
}
