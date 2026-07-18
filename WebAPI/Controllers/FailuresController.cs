using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para el registro de averías, alertas de seguridad y paradas de emergencia (§14.4).
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,Engineer")]
public class FailuresController : SgdeControllerBase
{
    private readonly FailureManager _failureManager = new();

    // Método manejador que registra una falla o incidente operativo cambiando la turbina a estado Damaged.
    [HttpPost("Register")]
    public IActionResult Register([FromBody] RegisterFailureRequest request)
    {
        _failureManager.Register(request, CallerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Falla reportada con éxito. Turbina detenida por seguridad." });
    }

    // Función de consulta que lista el historial de fallas reportadas para una turbina específica.
    [HttpGet("ByTurbine/{turbineId:int}")]
    public IActionResult GetByTurbine(int turbineId)
    {
        var result = _failureManager.RetrieveByTurbine(turbineId);
        return Ok(new ApiResponse<List<Failure>> { Success = true, Data = result });
    }

    // Función de consulta que retorna el listado global de fallas de todo el parque eólico (§14.4).
    [HttpGet("All")]
    public IActionResult GetAll()
    {
        var result = _failureManager.RetrieveAll();
        return Ok(new ApiResponse<List<Failure>> { Success = true, Data = result });
    }
}
