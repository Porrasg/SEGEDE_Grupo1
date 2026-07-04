namespace SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

/// <summary>
/// Acceso no autorizado → HTTP 403 (§5).
/// </summary>
public class UnauthorizedAccessAppException : Exception
{
    public string? Code { get; }

    public UnauthorizedAccessAppException()
        : base("You do not have permission for this operation.") { }

    public UnauthorizedAccessAppException(string message, string? code = null)
        : base(message) => Code = code;
}
