namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

/// <summary>
/// Registro de comprador (RF-001, §8.1).
/// </summary>
public class RegisterBuyerRequest
{
    public string Identification { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string Password { get; set; } = string.Empty;
}
