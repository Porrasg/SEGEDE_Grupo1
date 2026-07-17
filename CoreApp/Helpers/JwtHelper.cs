using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SEGEDE_Grupo1.CoreApp.Helpers;

// Proporciona utilidades para la generación y validación de tokens JWT (§13.2).
// Incluye claims estándar como NameIdentifier (userId), Email y Role.
public static class JwtHelper
{
    // Genera un token JWT firmado para el usuario con su rol y tiempo de expiración.
    // Parámetro userId: Identificador único del usuario (tblUser.Id).
    // Parámetro email: Correo electrónico del usuario.
    // Parámetro role: Rol del usuario en el sistema (Administrator, Engineer, Operations, Buyer).
    // Parámetro secret: Clave secreta simétrica (mínimo 32 caracteres/256 bits).
    // Parámetro expiryMinutes: Tiempo de validez del token en minutos.
    // Retorna: Token JWT codificado en formato de cadena.
    public static string GenerateToken(int userId, string email, string role, string secret, int expiryMinutes)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secret);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
