namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

// Establecer capacidad manual del banco central (RF-039, §8.1). null = limpiar.
public class SetManualCapacityRequest
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal? Capacity { get; set; }
}
