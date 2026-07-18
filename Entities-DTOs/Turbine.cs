namespace SEGEDE_Grupo1.EntitiesDTOs;

// Nota arquitectónica: Entidad Turbine mapeada a tblTurbines segÃºn documento tÃ©cnico Â§9.3.
public class Turbine : BaseDTO
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string UniqueCode { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Name { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Location { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Brand { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Model { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int Year { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal WeeklyNominalCapacity { get; set; }
    // Estado operativo actual o etapa en el ciclo de vida del registro.
    public string Status { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime? LastMaintenance { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime LastStateChange { get; set; }
}
