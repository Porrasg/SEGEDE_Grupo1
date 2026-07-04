namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Log de saturación → tblSaturationLog (§9.13). WORM.
/// </summary>
public class SaturationLog : BaseDTO
{
    public int FlushId { get; set; }
    public decimal PreviousInventory { get; set; }
    public decimal NewInventory { get; set; }
    public decimal ExcessEnergy { get; set; }
    public DateTime EventDate { get; set; }
}
