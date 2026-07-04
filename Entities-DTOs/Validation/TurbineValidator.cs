using SEGEDE_Grupo1.EntitiesDTOs.Helpers;

namespace SEGEDE_Grupo1.EntitiesDTOs.Validation;

/// <summary>
/// Validador de turbina (§6.2).
/// </summary>
public static class TurbineValidator
{
    public static ValidationResult Validate(
        string? uniqueCode, string? name, string? location,
        string? brand, string? model, int year, decimal weeklyNominalCapacity)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(uniqueCode))
            result.Add("UniqueCode is required.");
        else if (uniqueCode.Length > 50)
            result.Add("UniqueCode must not exceed 50 characters.");

        if (string.IsNullOrWhiteSpace(name))
            result.Add("Name is required.");

        if (string.IsNullOrWhiteSpace(location))
            result.Add("Location is required.");

        if (string.IsNullOrWhiteSpace(brand))
            result.Add("Brand is required.");

        if (string.IsNullOrWhiteSpace(model))
            result.Add("Model is required.");

        var currentYear = TimeHelper.NowCR().Year;
        if (year < 1900 || year > currentYear + 1)
            result.Add($"Year must be between 1900 and {currentYear + 1}.");

        if (weeklyNominalCapacity <= 0)
            result.Add("WeeklyNominalCapacity must be greater than 0.");
        else
        {
            // Verificar máximo 4 decimales
            var decimalPart = weeklyNominalCapacity % 1;
            if (decimalPart != 0 && decimalPart.ToString("G").Length - 2 > 4)
                result.Add("WeeklyNominalCapacity must have at most 4 decimal places.");
        }

        return result;
    }
}
