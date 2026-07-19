namespace SEGEDE_Grupo1.EntitiesDTOs;

// Nota arquitectónica: Entidad User mapeada a tblUsers segÃºn documento tÃ©cnico Â§9.1.
public class User : BaseDTO
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
    public string? PhotoUrl { get; set; }
    // Hash criptográfico de la contraseña del usuario (SHA-256 / PBKDF2).
    public string PasswordHash { get; set; } = string.Empty;
    // Rol de control de acceso basado en roles (RBAC): Administrator, Engineer o Buyer.
    public string Role { get; set; } = string.Empty;
    // Estado operativo actual o etapa en el ciclo de vida del registro.
    public string Status { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int FailedAttempts { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime? BlockedAt { get; set; }
}
