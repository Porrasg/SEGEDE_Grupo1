namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>
/// Actualización de usuario por Admin (RF-009, §8.1).
/// </summary>
public class UpdateUserRequest
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
