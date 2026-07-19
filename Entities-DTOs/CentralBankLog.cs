namespace SEGEDE_Grupo1.EntitiesDTOs;

// Log de movimientos del banco central → tblCentralBankLog (§9.15).
public class CentralBankLog : BaseDTO
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string MovementType { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal Amount { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal ResultingInventory { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int? FlushId { get; set; }
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int? DistributionId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime EventDate { get; set; }
}
