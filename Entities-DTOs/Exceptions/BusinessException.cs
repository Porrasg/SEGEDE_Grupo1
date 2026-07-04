namespace SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

// Regla de negocio violada → HTTP 409 (§5).
public class BusinessException : Exception
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string? Code { get; }

    public BusinessException(string message, string? code = null) : base(message)
        => Code = code;
}
