namespace SEGEDE_Grupo1.EntitiesDTOs;

// Precio de energía → tblPrice (§9.19). Solo uno activo a la vez.
public class Price : BaseDTO
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal PriceCRCPerMWh { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime ValidFrom { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime? ValidTo { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public bool IsActive { get; set; }
}
