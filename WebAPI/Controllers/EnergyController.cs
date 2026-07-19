using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para la simulación de generación, cálculo de pérdidas e inventario de baterías locales (§14.5).
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,Engineer")]
public class EnergyController : SgdeControllerBase
{
    private readonly EnergyManager _energyManager = new();

    [HttpPost("RunSimulation")]
    public IActionResult RunSimulation()
    {
        _energyManager.RunSimulationCycle();
        return Ok(new { message = "Ciclo de simulación de energía ejecutado correctamente." });
    }

    [HttpPost("SetBatteryCharge")]
    public IActionResult SetBatteryCharge([FromBody] SetBatteryChargeRequest request)
    {
        _energyManager.SetLocalBatteryCharge(request.TurbineId, request.StoredEnergy);
        return Ok(new { message = "Carga de batería local actualizada." });
    }

    [HttpGet("LocalBattery/{turbineId:int}")]
    public IActionResult GetLocalBattery(int turbineId)
    {
        var b = _energyManager.RetrieveLocalBattery(turbineId);
        return Ok(b);
    }

    [HttpGet("LocalBatteries/All")]
    public IActionResult GetAllLocalBatteries()
    {
        var list = _energyManager.RetrieveAllLocalBatteries();
        return Ok(list);
    }

    [HttpGet("GenerationHistory/{turbineId:int}")]
    public IActionResult GetGenerationHistory(int turbineId)
    {
        var result = _energyManager.RetrieveGenerationHistory(turbineId);
        return Ok(result);
    }

    [HttpGet("LossHistory/{turbineId:int}")]
    public IActionResult GetLossHistory(int turbineId)
    {
        var result = _energyManager.RetrieveLossHistory(turbineId);
        return Ok(result);
    }
}
