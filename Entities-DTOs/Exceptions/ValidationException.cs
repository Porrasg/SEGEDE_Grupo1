namespace SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

/// <summary>
/// Validación de inputs → HTTP 400 (§5).
/// </summary>
public class ValidationException : Exception
{
    public string[] Errors { get; }

    public ValidationException(string[] errors)
        : base("Validation errors.") => Errors = errors;
}
