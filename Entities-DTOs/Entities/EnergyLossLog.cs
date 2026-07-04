namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

// Log de pérdida de energía → tblEnergyLossLog (§9.9). Insert-only.
public class EnergyLossLog : BaseDTO
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int TurbineId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal InactiveTimeSeconds { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal LostEnergy { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Cause { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime EventDate { get; set; }
}
