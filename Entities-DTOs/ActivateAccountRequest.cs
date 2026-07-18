namespace SEGEDE_Grupo1.EntitiesDTOs;

// Activación de cuenta vía OTP (RF-004, §8.1).
public class ActivateAccountRequest
{
    // Correo electrónico principal utilizado como credencial y medio de notificación.
    public string Email { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string OtpCode { get; set; } = string.Empty;
}
