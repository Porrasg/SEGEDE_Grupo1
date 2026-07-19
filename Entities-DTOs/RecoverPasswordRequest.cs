namespace SEGEDE_Grupo1.EntitiesDTOs;

// Solicitud de recuperación de contraseña (RF-006, §8.1).
public class RecoverPasswordRequest
{
    // Correo electrónico principal utilizado como credencial y medio de notificación.
    public string Email { get; set; } = string.Empty;
}
