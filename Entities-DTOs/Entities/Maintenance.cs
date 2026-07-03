namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

// TODO: Entidad Maintenance mapeada a tblMaintenances segÃºn documento tÃ©cnico Â§9.6.
public class Maintenance : BaseDTO
{
    public int TurbineId { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public DateTime EstimatedStartDate { get; set; }
    public DateTime EstimatedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public string? Result { get; set; }
    public string Status { get; set; } = string.Empty;
}
