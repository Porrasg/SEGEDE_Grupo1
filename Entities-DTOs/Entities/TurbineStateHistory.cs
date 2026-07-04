namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Historial de estados de turbina → tblTurbineStateHistory (§9.4).
/// </summary>
public class TurbineStateHistory : BaseDTO
{
    public int TurbineId { get; set; }
    public string PreviousState { get; set; } = string.Empty;
    public string NewState { get; set; } = string.Empty;
    public DateTime ChangeDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int UserId { get; set; }
}
