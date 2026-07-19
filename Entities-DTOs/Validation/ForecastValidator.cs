using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.EntitiesDTOs.Validation;

// Validador de pronóstico de demanda (§6.4).
public static class ForecastValidator
{
    // Realiza la validación técnica y de reglas de negocio sobre los parámetros de entrada del sistema.
    public static ValidationResult Validate(decimal amountMWh, int month, int year)
    {
        var result = new ValidationResult();

        if (amountMWh <= 0)
            result.Add("AmountMWh must be greater than 0.");
        else
        {
            // Máximo 4 decimales
            var decimalPart = amountMWh % 1;
            if (decimalPart != 0 && decimalPart.ToString("G").Length - 2 > 4)
                result.Add("AmountMWh must have at most 4 decimal places.");
        }

        // (Month, Year) debe ser estrictamente futuro
        var now = TimeHelper.NowCR();
        if (year < now.Year || (year == now.Year && month <= now.Month))
            result.Add("Forecast month/year must be strictly in the future.");

        // Horizonte máximo: 6 meses adelante (RN-022)
        var forecastDate = new DateTime(year, month, 1);
        var maxDate = new DateTime(now.Year, now.Month, 1).AddMonths(6);
        if (forecastDate > maxDate)
            result.Add("Forecast cannot exceed 6 months into the future.");

        return result;
    }
}
