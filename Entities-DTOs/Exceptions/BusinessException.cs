namespace SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

// Regla de negocio violada → HTTP 409 (§5).
public class BusinessException : Exception
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string? Code { get; }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public BusinessException(string message, string? code = null) : base(message)
        => Code = code;
}
