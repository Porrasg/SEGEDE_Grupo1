using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;

/// <summary>Historial completo de una turbina (§8.2).</summary>
public class TurbineHistoryResponse
{
    public int TurbineId { get; set; }
    public List<TurbineStateHistory> StateChanges { get; set; } = new();
    public List<Maintenance> Maintenances { get; set; } = new();
    public List<Failure> Failures { get; set; } = new();
    public decimal TotalGeneratedEnergy { get; set; }
    public decimal TotalLostEnergy { get; set; }
}
