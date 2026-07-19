namespace SEGEDE_Grupo1.EntitiesDTOs;

// Flush de energía → tblFlush (§9.11).
public class Flush : BaseDTO
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string ExecutionType { get; set; } = string.Empty;
    // Estado operativo actual o etapa en el ciclo de vida del registro.
    public string Status { get; set; } = string.Empty;
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int? UserId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal TotalTransferredEnergy { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal SaturationLoss { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime StartDate { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime? EndDate { get; set; }
}
