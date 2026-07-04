namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

// Batería local de turbina → tblLocalBattery (§9.5). Relación 1:1 con Turbine.
public class LocalBattery : BaseDTO
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int TurbineId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal StoredEnergy { get; set; }
}
