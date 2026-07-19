namespace SEGEDE_Grupo1.CoreApp.Exceptions;

// Excepción lanzada cuando el usuario no tiene permiso para la operación → HTTP 403.
public class UnauthorizedAccessAppException : Exception
{
    public string? Code { get; }

    public UnauthorizedAccessAppException()
        : base("No tiene permiso para realizar esta operación.") { }

    public UnauthorizedAccessAppException(string message, string? code = null)
        : base(message) => Code = code;
}
