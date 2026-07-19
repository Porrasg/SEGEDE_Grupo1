namespace SEGEDE_Grupo1.EntitiesDTOs;

// Login paso 2 — verificación OTP (RF-005, §8.1).
public class LoginStep2Request
{
    // Correo electrónico principal utilizado como credencial y medio de notificación.
    public string Email { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string OtpCode { get; set; } = string.Empty;
}
