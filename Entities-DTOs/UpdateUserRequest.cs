namespace SEGEDE_Grupo1.EntitiesDTOs;

// Actualización de usuario por Admin (RF-009, §8.1).
public class UpdateUserRequest
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int UserId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string FirstName { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string LastName { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Phone { get; set; } = string.Empty;
    // Rol de control de acceso basado en roles (RBAC): Administrator, Engineer o Buyer.
    public string Role { get; set; } = string.Empty;
    // Estado operativo actual o etapa en el ciclo de vida del registro.
    public string Status { get; set; } = string.Empty;
}
