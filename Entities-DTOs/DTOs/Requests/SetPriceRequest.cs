namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>Establecer precio por MWh (RF-057, §8.1).</summary>
public class SetPriceRequest
{
    public decimal PriceCRCPerMWh { get; set; }
}
