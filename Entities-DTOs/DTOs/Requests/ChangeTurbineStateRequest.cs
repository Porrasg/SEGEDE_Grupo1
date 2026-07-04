namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

// Cambio de estado de turbina (RF-016, §8.1).
public class ChangeTurbineStateRequest
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int TurbineId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string NewState { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Reason { get; set; } = string.Empty;
}
