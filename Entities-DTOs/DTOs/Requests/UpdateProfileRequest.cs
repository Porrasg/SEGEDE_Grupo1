namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

// Actualización de perfil por Buyer (RF-010, §8.1).
public class UpdateProfileRequest
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Phone { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string? PhotoUrl { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string? NewPassword { get; set; }
}
