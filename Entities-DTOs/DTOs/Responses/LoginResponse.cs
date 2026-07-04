namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;

/// <summary>Respuesta de login exitoso con JWT (§8.2).</summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
}
