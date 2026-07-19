namespace SEGEDE_Grupo1.EntitiesDTOs;

// Registro de turbina (RF-013, §8.1).
public class RegisterTurbineRequest
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
}
