namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;

/// <summary>Respuesta segura de usuario. Nunca expone PasswordHash ni FailedAttempts (§8.2).</summary>
public class UserSafeResponse
{
    public int Id { get; set; }
    public string Identification { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public int Age { get; set; }
    public DateTime Created { get; set; }
}
