namespace SEGEDE_Grupo1.CoreApp.Helpers;

/// <summary>
/// Proporciona utilidades para el cifrado (hashing) y verificación de contraseñas
/// utilizando el algoritmo BCrypt con un factor de trabajo (workFactor) de 12 (§13.1).
/// </summary>
public static class PasswordHasher
{
    /// <summary>
    /// Genera un hash seguro para una contraseña en texto plano utilizando BCrypt.
    /// </summary>
    /// <param name="plain">Contraseña en texto plano.</param>
    /// <returns>Hash criptográfico BCrypt resultante.</returns>
    public static string Hash(string plain) =>
        BCrypt.Net.BCrypt.HashPassword(plain, workFactor: 12);

    /// <summary>
    /// Verifica si una contraseña en texto plano coincide con un hash BCrypt existente.
    /// </summary>
    /// <param name="plain">Contraseña en texto plano a verificar.</param>
    /// <param name="hash">Hash BCrypt almacenado.</param>
    /// <returns>True si la contraseña es válida; de lo contrario, False.</returns>
    public static bool Verify(string plain, string hash) =>
        BCrypt.Net.BCrypt.Verify(plain, hash);
}
