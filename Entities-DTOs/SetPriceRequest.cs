namespace SEGEDE_Grupo1.EntitiesDTOs;

// Establecer precio por MWh (RF-057, §8.1).
public class SetPriceRequest
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal PriceCRCPerMWh { get; set; }
}
