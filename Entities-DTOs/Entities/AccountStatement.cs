namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Estado de cuenta → tblAccountStatement (§9.21). WORM parcial.
/// Campos financieros congelados; solo Status y AnnulmentReason mutables.
/// </summary>
public class AccountStatement : BaseDTO
{
    public int BuyerId { get; set; }
    public int DistributionId { get; set; }
    public int ForecastId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal AssignedMWh { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public int RevisionNumber { get; set; }
    public int? ParentId { get; set; }
    public string? AnnulmentReason { get; set; }
    public DateTime IssueDate { get; set; }
}
