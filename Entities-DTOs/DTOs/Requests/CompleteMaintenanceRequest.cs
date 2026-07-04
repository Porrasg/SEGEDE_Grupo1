namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

// Completar mantenimiento (RF-017, §8.1).
public class CompleteMaintenanceRequest
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int MaintenanceId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Result { get; set; } = string.Empty;
}
