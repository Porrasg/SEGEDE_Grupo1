namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

// Impuesto → tblTax (§9.20). Solo uno activo a la vez.
// Percentage como fracción: 0.1300 = 13%.
public class Tax : BaseDTO
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Name { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal Percentage { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime ValidFrom { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime? ValidTo { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public bool IsActive { get; set; }
}
