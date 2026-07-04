namespace SEGEDE_Grupo1.EntitiesDTOs.Validation;

// Validador de facturación: precios, impuestos y anulaciones (§6.5).
public static class BillingValidator
{
    public static ValidationResult ValidatePrice(decimal priceCRCPerMWh)
    {
        var result = new ValidationResult();

        if (priceCRCPerMWh <= 0)
            result.Add("PriceCRCPerMWh must be greater than 0.");
        else
        {
            var decimalPart = priceCRCPerMWh % 1;
            if (decimalPart != 0 && decimalPart.ToString("G").Length - 2 > 4)
                result.Add("PriceCRCPerMWh must have at most 4 decimal places.");
        }

        return result;
    }

    public static ValidationResult ValidateTax(decimal percentage)
    {
        var result = new ValidationResult();

        if (percentage < 0)
            result.Add("Tax percentage must be >= 0.");
        else if (percentage >= 1)
            result.Add("Tax percentage must be < 1 (fraction: 0.13 = 13%).");

        return result;
    }

    public static ValidationResult ValidateAnnulment(string? reason)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(reason))
            result.Add("Annulment reason is required.");
        else if (reason.Length > 500)
            result.Add("Annulment reason must not exceed 500 characters.");

        return result;
    }
}
