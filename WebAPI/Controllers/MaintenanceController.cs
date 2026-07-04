using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp.Managers;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para la programación y control de mantenimientos preventivos y correctivos (§14.3).
[ApiController]
[Route("api/[controller]")]
public class MaintenanceController : ControllerBase
{
    private readonly MaintenanceManager _maintenanceManager = new();

    // Método manejador que agenda un nuevo mantenimiento preventivo o correctivo para una turbina.
    [HttpPost("Schedule")]
    public IActionResult Schedule([FromBody] RegisterMaintenanceRequest request, [FromQuery] int callerUserId = 1)
    {
        _maintenanceManager.Register(request, callerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Mantenimiento programado y turbina puesta en mantenimiento." });
    }

    // Método manejador que finaliza un mantenimiento en curso y reactiva la operación normal de la turbina.
    [HttpPost("Complete")]
    public IActionResult Complete([FromBody] CompleteMaintenanceRequest request, [FromQuery] int callerUserId = 1)
    {
        _maintenanceManager.Complete(request, callerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Mantenimiento completado y turbina reactivada." });
    }

    // Función de consulta que lista el historial de mantenimientos de una turbina en específico.
    [HttpGet("ByTurbine/{turbineId:int}")]
    public IActionResult GetByTurbine(int turbineId)
    {
        var result = _maintenanceManager.RetrieveByTurbine(turbineId);
        return Ok(new ApiResponse<List<Maintenance>> { Success = true, Data = result });
    }
}
