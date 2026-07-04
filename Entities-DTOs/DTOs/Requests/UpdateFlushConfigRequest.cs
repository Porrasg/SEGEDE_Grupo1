namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>Configuración de flush automático (RF-031, §8.1).</summary>
public class UpdateFlushConfigRequest
{
    public TimeSpan ExecutionTime { get; set; }
    public bool IsAutomatic { get; set; }
}
