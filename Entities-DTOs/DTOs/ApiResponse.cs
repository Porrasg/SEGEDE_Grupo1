namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs;

// Envoltura estándar para respuestas HTTP de la API (§3.3).
public class ApiResponse<T>
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public bool Success { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string? Message { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public T? Data { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string[]? Errors { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string? ErrorCode { get; set; }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public static ApiResponse<T> Ok(T data, string? msg = null) =>
        new() { Success = true, Data = data, Message = msg };

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public static ApiResponse<T> Fail(string msg, string? code = null, string[]? errors = null) =>
        new() { Success = false, Message = msg, ErrorCode = code, Errors = errors };
}
