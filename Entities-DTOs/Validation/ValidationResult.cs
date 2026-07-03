namespace SEGEDE_Grupo1.EntitiesDTOs.Validation;

// TODO: Resultado de validaciÃ³n para lÃ³gica de negocio y entidades segÃºn documento tÃ©cnico Â§6.
public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
}
