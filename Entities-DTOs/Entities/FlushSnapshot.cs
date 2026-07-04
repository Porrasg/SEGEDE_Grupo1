namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Snapshot de flush → tblFlushSnapshot (§9.12). WORM.
/// </summary>
public class FlushSnapshot : BaseDTO
{
    public int FlushId { get; set; }
    public int TurbineId { get; set; }
    public int LocalBatteryId { get; set; }
    public decimal CapturedEnergy { get; set; }
    public DateTime EventDate { get; set; }
}
