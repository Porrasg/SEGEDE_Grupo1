using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para el registro y modificación de pronósticos de consumo mensual por parte de compradores (§14.8).
[ApiController]
[Route("api/[controller]")]
public class ForecastController : SgdeControllerBase
{
    private readonly ForecastManager _forecastManager = new();

    // Método manejador que procesa el ingreso de un nuevo pronóstico de demanda energética mensual para un comprador. Solo Buyer.
    [Authorize(Roles = "Buyer")]
    [HttpPost("Register")]
    public IActionResult Register([FromBody] RegisterForecastRequest request)
    {
        _forecastManager.Register(request, CallerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Pronóstico de consumo registrado con éxito." });
    }

    // Método manejador que actualiza la cantidad de MWh de un pronóstico que se encuentre en estado pendiente. Solo Buyer (propio).
    [Authorize(Roles = "Buyer")]
    [HttpPut("Modify")]
    public IActionResult Modify([FromBody] ModifyForecastRequest request)
    {
        _forecastManager.Modify(request, CallerUserId, CallerRole);
        return Ok(new ApiResponse<object> { Success = true, Message = "Pronóstico de consumo actualizado." });
    }

    // Función de consulta que lista los pronósticos registrados para un mes y año específicos. Vista de Admin (Admin/Forecasts).
    [Authorize(Roles = "Administrator")]
    [HttpGet("ByMonth")]
    public IActionResult GetByMonth([FromQuery] int month, [FromQuery] int year)
    {
        var result = _forecastManager.RetrieveByMonth(month, year);
        return Ok(new ApiResponse<List<Forecast>> { Success = true, Data = result });
    }

    // Función de consulta que recupera el listado de pronósticos de demanda de un comprador específico (§14.8). Ownership: Buyer propio o Admin.
    [Authorize(Roles = "Administrator,Buyer")]
    [HttpGet("ByBuyer/{buyerId:int}")]
    public IActionResult GetByBuyer(int buyerId)
    {
        var result = _forecastManager.RetrieveByBuyer(buyerId, CallerUserId, CallerRole);
        return Ok(new ApiResponse<List<Forecast>> { Success = true, Data = result });
    }

    // Método manejador que ejecuta la cancelación formal de un pronóstico de demanda futuro. Solo Buyer (propio).
    [Authorize(Roles = "Buyer")]
    [HttpPost("Cancel/{forecastId:int}")]
    public IActionResult Cancel(int forecastId)
    {
        _forecastManager.Cancel(forecastId, CallerUserId, CallerRole);
        return Ok(new ApiResponse<object> { Success = true, Message = "Pronóstico cancelado con éxito." });
    }
}
