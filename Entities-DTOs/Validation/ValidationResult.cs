using SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

namespace SEGEDE_Grupo1.EntitiesDTOs.Validation;

// Resultado de validación para lógica de negocio (§6).
public class ValidationResult
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public List<string> Errors { get; } = new();
    public bool IsValid => Errors.Count == 0;

    public void Add(string error) => Errors.Add(error);

    public void ThrowIfInvalid()
    {
        if (!IsValid) throw new Exceptions.ValidationException(Errors.ToArray());
    }
}
