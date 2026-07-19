namespace SEGEDE_Grupo1.EntitiesDTOs;

// Creación de usuario interno por Admin (RF-008, §8.1).
public class CreateInternalUserRequest
{
    // Número de cédula o documento de identificación fiscal del usuario.
    public string Identification { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string FirstName { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string LastName { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime BirthDate { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Phone { get; set; } = string.Empty;
    // Correo electrónico principal utilizado como credencial y medio de notificación.
    public string Email { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Password { get; set; } = string.Empty;
    // Rol de control de acceso basado en roles (RBAC): Administrator, Engineer o Buyer.
    public string Role { get; set; } = string.Empty;
}
