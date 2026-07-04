namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Flush de energía → tblFlush (§9.11).
/// </summary>
public class Flush : BaseDTO
{
    public string ExecutionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public decimal TotalTransferredEnergy { get; set; }
    public decimal SaturationLoss { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
