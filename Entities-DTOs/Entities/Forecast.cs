namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

// Pronóstico de demanda → tblForecast (§9.16).
// UNIQUE (BuyerId, Month, Year) — solo un forecast activo por combinación.
public class Forecast : BaseDTO
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int BuyerId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int Month { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int Year { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal AmountMWh { get; set; }
    // Estado operativo actual o etapa en el ciclo de vida del registro.
    public string Status { get; set; } = string.Empty;
}
