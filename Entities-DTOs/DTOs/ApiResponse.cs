namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs;

// TODO: Envoltura estÃ¡ndar para respuestas HTTP de la API segÃºn documento tÃ©cnico Â§3.3.
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public string[]? Errors { get; set; }
    public string? ErrorCode { get; set; }
}
