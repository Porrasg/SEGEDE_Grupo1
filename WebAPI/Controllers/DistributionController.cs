using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp.Managers;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para la ejecución de la distribución comercial mensual y cálculo de escasez o suficiencia (§14.9).
[ApiController]
[Route("api/[controller]")]
public class DistributionController : ControllerBase
{
    private readonly DistributionManager _distributionManager = new();

    // Método manejador que ejecuta el proceso de cierre y distribución de energía para el mes y año solicitados.
    [HttpPost("ExecuteMonthly")]
    public IActionResult ExecuteMonthly([FromQuery] int month, [FromQuery] int year)
    {
        _distributionManager.RunMonthlyDistribution(month, year);
        return Ok(new ApiResponse<object> { Success = true, Message = "Distribución comercial calculada y cerrada satisfactoriamente." });
    }

    // Función de consulta que retorna el historial de las distribuciones comerciales cerradas.
    [HttpGet("History")]
    public IActionResult GetHistory()
    {
        var result = _distributionManager.RetrieveHistory();
        return Ok(new ApiResponse<List<CommercialDistribution>> { Success = true, Data = result });
    }

    // Función de consulta que recupera el historial de distribuciones y asignaciones energéticas de un comprador específico (§14.9).
    [HttpGet("ByBuyer/{buyerId:int}")]
    public IActionResult GetByBuyer(int buyerId, [FromQuery] int callerUserId = 1, [FromQuery] string callerRole = "Buyer")
    {
        var result = _distributionManager.RetrieveDetailByBuyer(buyerId, callerUserId, callerRole);
        return Ok(new ApiResponse<List<DistributionDetail>> { Success = true, Data = result });
    }
}
