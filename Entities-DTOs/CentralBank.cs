namespace SEGEDE_Grupo1.EntitiesDTOs;

// Banco central de energía → tblCentralBank (§9.14). Singleton (Id=1).
// EffectiveCapacity = ManualCapacity ?? AutomaticCapacity (calculada, no persistida).
public class CentralBank : BaseDTO
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal CurrentInventory { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal? ManualCapacity { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal AutomaticCapacity { get; set; }
    public decimal EffectiveCapacity => ManualCapacity ?? AutomaticCapacity;
}
