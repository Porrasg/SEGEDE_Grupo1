namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Banco central de energía → tblCentralBank (§9.14). Singleton (Id=1).
/// EffectiveCapacity = ManualCapacity ?? AutomaticCapacity (calculada, no persistida).
/// </summary>
public class CentralBank : BaseDTO
{
    public decimal CurrentInventory { get; set; }
    public decimal? ManualCapacity { get; set; }
    public decimal AutomaticCapacity { get; set; }
    public decimal EffectiveCapacity => ManualCapacity ?? AutomaticCapacity;
}
