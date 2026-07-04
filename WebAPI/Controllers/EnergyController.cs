using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp.Managers;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para la simulación de generación, cálculo de pérdidas e inventario de baterías locales (§14.5).
[ApiController]
[Route("api/[controller]")]
public class EnergyController : ControllerBase
{
    private readonly EnergyManager _energyManager = new();

    // Método manejador que ejecuta manualmente el ciclo de simulación de energía y cálculo de pérdidas.
    [HttpPost("RunSimulation")]
    public IActionResult RunSimulation()
    {
        _energyManager.RunSimulationCycle();
        return Ok(new ApiResponse<object> { Success = true, Message = "Ciclo de simulación de energía ejecutado correctamente." });
    }

    // Función de consulta que obtiene el estado actual y nivel de carga de la batería local de una turbina.
    [HttpGet("LocalBattery/{turbineId:int}")]
    public IActionResult GetLocalBattery(int turbineId)
    {
        var b = _energyManager.RetrieveLocalBattery(turbineId);
        return Ok(new ApiResponse<LocalBattery> { Success = true, Data = b });
    }

    // Función de consulta que retorna el historial paginado de generación de energía de una turbina.
    [HttpGet("GenerationHistory/{turbineId:int}")]
    public IActionResult GetGenerationHistory(int turbineId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = _energyManager.RetrieveGenerationHistory(turbineId, new PagedRequest { Page = page, PageSize = pageSize });
        return Ok(new ApiResponse<PagedResponse<EnergyGenerationLog>> { Success = true, Data = result });
    }

    // Función de consulta que retorna el historial paginado de pérdidas energéticas de una turbina.
    [HttpGet("LossHistory/{turbineId:int}")]
    public IActionResult GetLossHistory(int turbineId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = _energyManager.RetrieveLossHistory(turbineId, new PagedRequest { Page = page, PageSize = pageSize });
        return Ok(new ApiResponse<PagedResponse<EnergyLossLog>> { Success = true, Data = result });
    }
}
