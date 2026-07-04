namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

// Reset de contraseña con OTP (RF-006, §8.1).
public class ResetPasswordRequest
{
    // Correo electrónico principal utilizado como credencial y medio de notificación.
    public string Email { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string OtpCode { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string NewPassword { get; set; } = string.Empty;
}
