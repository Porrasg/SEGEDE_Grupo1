using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp.Managers;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para la administración del catálogo de turbinas, telemetría y cambios de estado (§14.2).
[ApiController]
[Route("api/[controller]")]
public class TurbinesController : ControllerBase
{
    private readonly TurbineManager _turbineManager = new();

    // Método manejador que procesa el registro e incorporación de una nueva turbina al parque de generación.
    [HttpPost("Register")]
    public IActionResult Register([FromBody] RegisterTurbineRequest request, [FromQuery] int callerUserId = 1)
    {
        _turbineManager.Register(request, callerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Turbina registrada con éxito en el sistema." });
    }

    // Método manejador que procesa la actualización de características y capacidad nominal de una turbina.
    [HttpPut("Update")]
    public IActionResult Update([FromBody] UpdateTurbineRequest request, [FromQuery] int callerUserId = 1)
    {
        _turbineManager.Update(request, callerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Datos de la turbina actualizados." });
    }

    // Método manejador que ejecuta la transición de estado operativo de la turbina registrando su historial.
    [HttpPost("ChangeState")]
    public IActionResult ChangeState([FromBody] ChangeTurbineStateRequest request, [FromQuery] int callerUserId = 1)
    {
        _turbineManager.ChangeState(request, callerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Cambio de estado operativo ejecutado con éxito." });
    }

    // Función de consulta que retorna los datos completos de una turbina específica por su identificador.
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var t = _turbineManager.RetrieveById(id);
        return Ok(new ApiResponse<Turbine> { Success = true, Data = t });
    }

    // Función de consulta que obtiene el catálogo completo y paginado de todas las turbinas registradas.
    [HttpGet("RetrieveAll")]
    public IActionResult RetrieveAll()
    {
        var result = _turbineManager.RetrieveAll();
        return Ok(new ApiResponse<List<Turbine>> { Success = true, Data = result });
    }

    // Función de consulta que calcula y retorna las métricas operativas y de rendimiento de una turbina.
    [HttpGet("Metrics/{id:int}")]
    public IActionResult GetMetrics(int id)
    {
        var m = _turbineManager.RetrieveMetrics(id);
        return Ok(new ApiResponse<TurbineMetricsResponse> { Success = true, Data = m });
    }
}
