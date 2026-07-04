using SEGEDE_Grupo1.EntitiesDTOs.Constants;

namespace SEGEDE_Grupo1.EntitiesDTOs.Validation;

// Validador de mantenimiento (§6.3).
public static class MaintenanceValidator
{
    private static readonly HashSet<string> ValidTypes = new()
    {
        MaintenanceTypes.Preventive,
        MaintenanceTypes.Corrective
    };

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
