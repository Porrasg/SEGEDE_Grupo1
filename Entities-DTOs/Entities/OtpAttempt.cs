namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

// Intento de OTP → tblOtpAttempts (§9.2).
public class OtpAttempt : BaseDTO
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int UserId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string UsageType { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int ResendCount { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int FailedAttempts { get; set; }
    // Estado operativo actual o etapa en el ciclo de vida del registro.
    public string Status { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime StartDate { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime WindowExpiration { get; set; }
}
