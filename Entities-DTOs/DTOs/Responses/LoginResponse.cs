namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;

// Respuesta de login exitoso con JWT (§8.2).
public class LoginResponse
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Token { get; set; } = string.Empty;
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int UserId { get; set; }
    // Correo electrónico principal utilizado como credencial y medio de notificación.
    public string Email { get; set; } = string.Empty;
    // Rol de control de acceso basado en roles (RBAC): Administrator, Engineer o Buyer.
    public string Role { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime Expiration { get; set; }
}
