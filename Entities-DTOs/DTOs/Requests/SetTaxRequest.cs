namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

// Establecer impuesto (RF-058, §8.1).
public class SetTaxRequest
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Name { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal Percentage { get; set; }
}
