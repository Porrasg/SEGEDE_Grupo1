namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

// TODO: Entidad User mapeada a tblUsers segÃºn documento tÃ©cnico Â§9.1.
public class User : BaseDTO
{
    public string Identification { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int FailedAttempts { get; set; }
    public DateTime? BlockedAt { get; set; }
}
