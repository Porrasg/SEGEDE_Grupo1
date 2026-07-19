using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para la programación y control de mantenimientos preventivos y correctivos (§14.3).
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,Engineer")]
public class MaintenanceController : SgdeControllerBase
{
    private readonly MaintenanceManager _maintenanceManager = new();

    // Método manejador que agenda un nuevo mantenimiento preventivo o correctivo para una turbina. Operado por Engineer.
    [Authorize(Roles = "Engineer")]
    [HttpPost("Schedule")]
    public IActionResult Schedule([FromBody] RegisterMaintenanceRequest request)
    {
        _maintenanceManager.Register(request, CallerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Mantenimiento programado y turbina puesta en mantenimiento." });
    }

    // Método manejador que finaliza un mantenimiento en curso y reactiva la operación normal de la turbina. Operado por Engineer.
    [Authorize(Roles = "Engineer")]
    [HttpPost("Complete")]
    public IActionResult Complete([FromBody] CompleteMaintenanceRequest request)
    {
        _maintenanceManager.Complete(request, CallerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Mantenimiento completado y turbina reactivada." });
    }

    // Método manejador que cancela un mantenimiento programado (solo permitido en estado Scheduled). Operado por Engineer.
    // Ruta faltante detectada al revisar OperationsComplementaryViewController.js — MaintenanceManager.Cancel ya existía sin endpoint.
    [Authorize(Roles = "Engineer")]
    [HttpPost("Cancel/{maintenanceId:int}")]
    public IActionResult Cancel(int maintenanceId)
    {
        _maintenanceManager.Cancel(maintenanceId, CallerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Mantenimiento cancelado con éxito." });
    }

    // Función de consulta que lista el historial de mantenimientos de una turbina en específico.
    [HttpGet("ByTurbine/{turbineId:int}")]
    public IActionResult GetByTurbine(int turbineId)
    {
        var result = _maintenanceManager.RetrieveByTurbine(turbineId);
        return Ok(new ApiResponse<List<Maintenance>> { Success = true, Data = result });
    }

    // Función de consulta que retorna el listado global de mantenimientos de todas las turbinas (§14.3).
    [HttpGet("All")]
    public IActionResult GetAll()
    {
        var result = _maintenanceManager.RetrieveAll();
        return Ok(new ApiResponse<List<Maintenance>> { Success = true, Data = result });
    }
}
