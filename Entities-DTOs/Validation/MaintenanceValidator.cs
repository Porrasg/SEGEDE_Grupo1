using SEGEDE_Grupo1.EntitiesDTOs.Constants;

namespace SEGEDE_Grupo1.EntitiesDTOs.Validation;

// Validador de mantenimiento (§6.3).
public static class MaintenanceValidator
{
    // Ejecuta operaciones criptográficas para el resguardo y verificación segura de credenciales e integridad.
    private static readonly HashSet<string> ValidTypes = new()
    {
        MaintenanceTypes.Preventive,
        MaintenanceTypes.Corrective
    };

    // Realiza la validación técnica y de reglas de negocio sobre los parámetros de entrada del sistema.
    public static ValidationResult Validate(
        string? maintenanceType, DateTime estimatedStartDate, DateTime estimatedEndDate)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(maintenanceType))
            result.Add("MaintenanceType is required.");
        else if (!ValidTypes.Contains(maintenanceType))
            result.Add("MaintenanceType must be 'Preventive' or 'Corrective'.");

        if (estimatedStartDate >= estimatedEndDate)
            result.Add("EstimatedStartDate must be before EstimatedEndDate.");

        return result;
    }
}
