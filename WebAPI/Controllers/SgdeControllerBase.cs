using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Base compartida para todos los controllers protegidos: expone la identidad real del caller
// a partir de los claims del JWT validado por el middleware de autenticación (§10/§54),
// reemplazando los antiguos parámetros callerUserId/callerRole de query-string.
[Authorize]
public abstract class SgdeControllerBase : ControllerBase
{
    protected int CallerUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new UnauthorizedAccessAppException("Token sin identificador de usuario válido.");

    protected string CallerRole =>
        User.FindFirstValue(ClaimTypes.Role)
            ?? throw new UnauthorizedAccessAppException("Token sin rol asignado.");

    // Exige que el caller sea Administrator o que targetUserId coincida con su propio Id (ownership).
    protected void RequireOwnershipOrAdmin(int targetUserId)
    {
        if (CallerRole != "Administrator" && CallerUserId != targetUserId)
        {
            throw new UnauthorizedAccessAppException("No tiene permiso para acceder a datos de otro usuario.");
        }
    }
}
