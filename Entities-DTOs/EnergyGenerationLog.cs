namespace SEGEDE_Grupo1.EntitiesDTOs;

// Log de generación de energía → tblEnergyGenerationLog (§9.8). Insert-only.
public class EnergyGenerationLog : BaseDTO
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int TurbineId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal ActiveTimeSeconds { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal GeneratedEnergy { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime EventDate { get; set; }
}
