namespace SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

// Acceso no autorizado → HTTP 403 (§5).
public class UnauthorizedAccessAppException : Exception
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string? Code { get; }

    public UnauthorizedAccessAppException()
        : base("You do not have permission for this operation.") { }

    public UnauthorizedAccessAppException(string message, string? code = null)
        : base(message) => Code = code;
}
