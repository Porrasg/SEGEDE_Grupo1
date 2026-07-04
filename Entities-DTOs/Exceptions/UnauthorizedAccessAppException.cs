namespace SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

// Acceso no autorizado → HTTP 403 (§5).
public class UnauthorizedAccessAppException : Exception
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string? Code { get; }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public UnauthorizedAccessAppException()
        : base("You do not have permission for this operation.") { }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public UnauthorizedAccessAppException(string message, string? code = null)
        : base(message) => Code = code;
}
