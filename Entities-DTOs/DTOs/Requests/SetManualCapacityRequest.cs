namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>Establecer capacidad manual del banco central (RF-039, §8.1). null = limpiar.</summary>
public class SetManualCapacityRequest
{
    public decimal? Capacity { get; set; }
}
