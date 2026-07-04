namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

// Desactivación de usuario (RF-011, §8.1).
public class DeactivateUserRequest
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int UserId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public bool CancelForecasts { get; set; }
}
