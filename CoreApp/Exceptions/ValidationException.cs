namespace SEGEDE_Grupo1.CoreApp.Exceptions;

// Excepción lanzada cuando los datos de entrada no superan la validación → HTTP 400.
public class ValidationException : Exception
{
    public string[] Errors { get; }

    public ValidationException(string[] errors)
        : base("Errores de validación en los datos ingresados.") => Errors = errors;
}
