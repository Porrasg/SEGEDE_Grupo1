using System.Text.RegularExpressions;
using SEGEDE_Grupo1.EntitiesDTOs.Helpers;

namespace SEGEDE_Grupo1.EntitiesDTOs.Validation;

// Validador de usuario (§6.1).
public static class UserValidator
{
    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private static readonly Regex PasswordRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$", RegexOptions.Compiled);

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private static readonly Regex PhoneRegex = new(
        @"^(\+506)?\d{8}$", RegexOptions.Compiled);

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private static readonly Regex DigitsOnly = new(
        @"^\d+$", RegexOptions.Compiled);

    // Realiza la validación técnica y de reglas de negocio sobre los parámetros de entrada del sistema.
    public static ValidationResult Validate(
        string? email, string? identification, string? password,
        string? phone, DateTime? birthDate, string? firstName, string? lastName)
    {
        var result = new ValidationResult();

        // Email
        if (string.IsNullOrWhiteSpace(email))
            result.Add("Email is required.");
        else if (email.Length > 250)
            result.Add("Email must not exceed 250 characters.");
        else if (!EmailRegex.IsMatch(email))
            result.Add("Email format is invalid.");

        // Identification
        if (string.IsNullOrWhiteSpace(identification))
            result.Add("Identification is required.");
        else if (!DigitsOnly.IsMatch(identification))
            result.Add("Identification must contain only digits.");
        else if (identification.Length != 9 && identification.Length != 10
                 && identification.Length != 11 && identification.Length != 12)
            result.Add("Identification must be 9 (física), 10 (jurídica), 11 or 12 (DIMEX) digits.");

        // Password
        if (string.IsNullOrWhiteSpace(password))
            result.Add("Password is required.");
        else if (!PasswordRegex.IsMatch(password))
            result.Add("Password must be at least 8 characters with 1 uppercase, 1 lowercase, 1 digit, and 1 special character.");

        // Phone
        if (string.IsNullOrWhiteSpace(phone))
            result.Add("Phone is required.");
        else if (!PhoneRegex.IsMatch(phone))
            result.Add("Phone must be 8 digits, optionally prefixed with +506.");

        // BirthDate
        if (birthDate.HasValue)
        {
            var now = TimeHelper.NowCR();
            if (birthDate.Value > now)
                result.Add("Birth date cannot be in the future.");
            else
            {
                int age = now.Year - birthDate.Value.Year;
                if (birthDate.Value.Date > now.AddYears(-age)) age--;
                if (age < 18)
                    result.Add("User must be at least 18 years old.");
            }
        }

        // FirstName
        if (string.IsNullOrWhiteSpace(firstName))
            result.Add("First name is required.");
        else if (firstName.Length > 150)
            result.Add("First name must not exceed 150 characters.");

        // LastName
        if (string.IsNullOrWhiteSpace(lastName))
            result.Add("Last name is required.");
        else if (lastName.Length > 150)
            result.Add("Last name must not exceed 150 characters.");

        return result;
    }
}
