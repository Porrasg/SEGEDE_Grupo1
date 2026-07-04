namespace SEGEDE_Grupo1.CoreApp.Helpers;

// Proporciona utilidades para el cifrado (hashing) y verificación de contraseñas
// utilizando el algoritmo BCrypt con un factor de trabajo (workFactor) de 12 (§13.1).
public static class PasswordHasher
{
    // Genera un hash seguro para una contraseña en texto plano utilizando BCrypt.
    // Parámetro plain: Contraseña en texto plano.
    // Retorna: Hash criptográfico BCrypt resultante.
    public static string Hash(string plain) =>
        BCrypt.Net.BCrypt.HashPassword(plain, workFactor: 12);

    // Verifica si una contraseña en texto plano coincide con un hash BCrypt existente.
    // Parámetro plain: Contraseña en texto plano a verificar.
    // Parámetro hash: Hash BCrypt almacenado.
    // Retorna: True si la contraseña es válida; de lo contrario, False.
    public static bool Verify(string plain, string hash) =>
        BCrypt.Net.BCrypt.Verify(plain, hash);
}
