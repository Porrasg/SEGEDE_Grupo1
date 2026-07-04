namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Batería local de turbina → tblLocalBattery (§9.5). Relación 1:1 con Turbine.
/// </summary>
public class LocalBattery : BaseDTO
{
    public int TurbineId { get; set; }
    public decimal StoredEnergy { get; set; }
}
