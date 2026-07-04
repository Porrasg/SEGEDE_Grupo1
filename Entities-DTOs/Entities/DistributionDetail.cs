namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

// Detalle de distribución por comprador → tblDistributionDetail (§9.18).
public class DistributionDetail : BaseDTO
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int DistributionId { get; set; }
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int BuyerId { get; set; }
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int ForecastId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal RequestedMWh { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal AssignedMWh { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal UnsuppliedDemand { get; set; }
}
