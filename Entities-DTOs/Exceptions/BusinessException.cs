namespace SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

/// <summary>
/// Regla de negocio violada → HTTP 409 (§5).
/// </summary>
public class BusinessException : Exception
{
    public string? Code { get; }

    public BusinessException(string message, string? code = null) : base(message)
        => Code = code;
}
