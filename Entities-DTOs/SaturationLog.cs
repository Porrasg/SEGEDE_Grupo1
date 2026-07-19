namespace SEGEDE_Grupo1.EntitiesDTOs;

// Log de saturación → tblSaturationLog (§9.13). WORM.
public class SaturationLog : BaseDTO
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int FlushId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal PreviousInventory { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal NewInventory { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal ExcessEnergy { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime EventDate { get; set; }
}
