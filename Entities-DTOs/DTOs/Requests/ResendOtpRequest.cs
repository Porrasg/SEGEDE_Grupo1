namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

// Reenvío de OTP (RF-007, §8.1).
public class ResendOtpRequest
{
    // Correo electrónico principal utilizado como credencial y medio de notificación.
    public string Email { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string UsageType { get; set; } = string.Empty;
}
