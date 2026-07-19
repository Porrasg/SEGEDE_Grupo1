namespace SEGEDE_Grupo1.EntitiesDTOs;

// Respuesta segura de usuario. Nunca expone PasswordHash ni FailedAttempts (§8.2).
public class UserSafeResponse
{
    // Identificador único primario (PK) en la tabla relacional.
    public int Id { get; set; }
    // Número de cédula o documento de identificación fiscal del usuario.
    public string Identification { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string FirstName { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string LastName { get; set; } = string.Empty;
    // Correo electrónico principal utilizado como credencial y medio de notificación.
    public string Email { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Phone { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string? PhotoUrl { get; set; }
    // Rol de control de acceso basado en roles (RBAC): Administrator, Engineer o Buyer.
    public string Role { get; set; } = string.Empty;
    // Estado operativo actual o etapa en el ciclo de vida del registro.
    public string Status { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime BirthDate { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int Age { get; set; }
    // Marca de tiempo (UTC/Local) en que se creó el registro de auditoría.
    public DateTime Created { get; set; }
}
