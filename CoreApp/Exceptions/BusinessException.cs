namespace SEGEDE_Grupo1.CoreApp.Exceptions;

// Excepción lanzada cuando se viola una regla de negocio → HTTP 400/409.
public class BusinessException : Exception
{
    public string? Code { get; }

    public BusinessException(string message, string? code = null) : base(message)
        => Code = code;
}
