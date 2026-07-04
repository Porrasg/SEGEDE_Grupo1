namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Precio de energía → tblPrice (§9.19). Solo uno activo a la vez.
/// </summary>
public class Price : BaseDTO
{
    public decimal PriceCRCPerMWh { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
}
