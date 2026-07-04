namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs;

/// <summary>
/// Envoltura estándar para respuestas HTTP de la API (§3.3).
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public string[]? Errors { get; set; }
    public string? ErrorCode { get; set; }

    public static ApiResponse<T> Ok(T data, string? msg = null) =>
        new() { Success = true, Data = data, Message = msg };

    public static ApiResponse<T> Fail(string msg, string? code = null, string[]? errors = null) =>
        new() { Success = false, Message = msg, ErrorCode = code, Errors = errors };
}
