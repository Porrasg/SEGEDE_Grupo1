namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Impuesto → tblTax (§9.20). Solo uno activo a la vez.
/// Percentage como fracción: 0.1300 = 13%.
/// </summary>
public class Tax : BaseDTO
{
    public string Name { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
}
