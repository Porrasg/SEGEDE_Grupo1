namespace SEGEDE_Grupo1.EntitiesDTOs;

// Modificación de pronóstico (RF-046, §8.1).
public class ModifyForecastRequest
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int ForecastId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal NewAmountMWh { get; set; }
}
