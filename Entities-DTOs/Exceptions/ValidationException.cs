namespace SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

// Validación de inputs → HTTP 400 (§5).
public class ValidationException : Exception
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string[] Errors { get; }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public ValidationException(string[] errors)
        : base("Validation errors.") => Errors = errors;
}
