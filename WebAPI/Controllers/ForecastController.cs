using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp.Managers;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para el registro y modificación de pronósticos de consumo mensual por parte de compradores (§14.8).
[ApiController]
[Route("api/[controller]")]
public class ForecastController : ControllerBase
{
    private readonly ForecastManager _forecastManager = new();

    // Método manejador que procesa el ingreso de un nuevo pronóstico de demanda energética mensual para un comprador.
    [HttpPost("Register")]
    public IActionResult Register([FromBody] RegisterForecastRequest request, [FromQuery] int callerUserId = 1)
    {
        _forecastManager.Register(request, callerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Pronóstico de consumo registrado con éxito." });
    }

    // Método manejador que actualiza la cantidad de MWh de un pronóstico que se encuentre en estado pendiente.
    [HttpPut("Modify")]
    public IActionResult Modify([FromBody] ModifyForecastRequest request, [FromQuery] int callerUserId = 1, [FromQuery] string callerRole = "Buyer")
    {
        _forecastManager.Modify(request, callerUserId, callerRole);
        return Ok(new ApiResponse<object> { Success = true, Message = "Pronóstico de consumo actualizado." });
    }

    // Función de consulta que lista los pronósticos registrados para un mes y año específicos.
    [HttpGet("ByMonth")]
    public IActionResult GetByMonth([FromQuery] int month, [FromQuery] int year)
    {
        var result = _forecastManager.RetrieveByMonth(month, year);
        return Ok(new ApiResponse<List<Forecast>> { Success = true, Data = result });
    }
}
