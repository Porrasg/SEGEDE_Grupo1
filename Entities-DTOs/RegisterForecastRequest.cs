namespace SEGEDE_Grupo1.EntitiesDTOs;

// Registro de pronóstico de demanda (RF-044, §8.1).
public class RegisterForecastRequest
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int Month { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int Year { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal AmountMWh { get; set; }
}
