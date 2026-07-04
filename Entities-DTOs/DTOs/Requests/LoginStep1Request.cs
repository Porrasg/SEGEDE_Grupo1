namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

// Login paso 1 — email + password (RF-005, §8.1).
public class LoginStep1Request
{
    // Correo electrónico principal utilizado como credencial y medio de notificación.
    public string Email { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Password { get; set; } = string.Empty;
}
