namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Log de movimientos del banco central → tblCentralBankLog (§9.15).
/// </summary>
public class CentralBankLog : BaseDTO
{
    public string MovementType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal ResultingInventory { get; set; }
    public int? FlushId { get; set; }
    public int? DistributionId { get; set; }
    public DateTime EventDate { get; set; }
}
