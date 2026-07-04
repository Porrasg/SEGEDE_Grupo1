namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>Establecer impuesto (RF-058, §8.1).</summary>
public class SetTaxRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
}
