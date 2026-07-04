namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

// Estado de cuenta → tblAccountStatement (§9.21). WORM parcial.
// Campos financieros congelados; solo Status y AnnulmentReason mutables.
public class AccountStatement : BaseDTO
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int BuyerId { get; set; }
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int DistributionId { get; set; }
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int ForecastId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int Month { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int Year { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal AssignedMWh { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal UnitPrice { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal TaxPercentage { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal Subtotal { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal TaxAmount { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal Total { get; set; }
    // Estado operativo actual o etapa en el ciclo de vida del registro.
    public string Status { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int RevisionNumber { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int? ParentId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string? AnnulmentReason { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime IssueDate { get; set; }
}
