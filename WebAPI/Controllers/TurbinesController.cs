using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para la administración del catálogo de turbinas, telemetría y cambios de estado (§14.2).
// Solo Admin/Engineer interactúan con este dominio (Buyer no tiene páginas de turbinas).
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,Engineer")]
public class TurbinesController : SgdeControllerBase
{
    private readonly TurbineManager _turbineManager = new();

    // Método manejador que procesa el registro e incorporación de una nueva turbina al parque de generación. Solo Admin (TRB-07).
    [Authorize(Roles = "Administrator")]
    [HttpPost("Register")]
    public IActionResult Register([FromBody] RegisterTurbineRequest request)
    {
        _turbineManager.Register(request, CallerUserId);
        return Ok(new { message = "Turbina registrada con éxito en el sistema." });
    }

    // Método manejador que procesa la actualización de características y capacidad nominal de una turbina. Solo Admin.
    [Authorize(Roles = "Administrator")]
    [HttpPut("Update")]
    public IActionResult Update([FromBody] UpdateTurbineRequest request)
    {
        _turbineManager.Update(request, CallerUserId);
        return Ok(new { message = "Datos de la turbina actualizados." });
    }

    // Método manejador que ejecuta la transición de estado operativo de la turbina registrando su historial.
    [HttpPost("ChangeState")]
    public IActionResult ChangeState([FromBody] ChangeTurbineStateRequest request)
    {
        _turbineManager.ChangeState(request, CallerUserId);
        return Ok(new { message = "Cambio de estado operativo ejecutado con éxito." });
    }

    // Función de consulta que retorna los datos completos de una turbina específica por su identificador.
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var t = _turbineManager.RetrieveById(id);
        return Ok(t);
    }

    // Función de consulta que obtiene el catálogo completo y paginado de todas las turbinas registradas.
    [HttpGet("RetrieveAll")]
    [HttpGet("All")]
    public IActionResult RetrieveAll()
    {
        var result = _turbineManager.RetrieveAll();
        return Ok(result);
    }

    // Función de consulta que calcula y retorna las métricas operativas y de rendimiento de una turbina.
    [HttpGet("Metrics/{id:int}")]
    public IActionResult GetMetrics(int id)
    {
        var m = _turbineManager.RetrieveMetrics(id);
        return Ok(m);
    }

    // Función de consulta que recupera el historial operativo y de mantenimiento de una turbina (§14.2).
    [HttpGet("History/{id:int}")]
    public IActionResult GetHistory(int id)
    {
        var h = _turbineManager.RetrieveHistory(id);
        return Ok(h);
    }

    // Método manejador que fuerza la verificación de mantenimiento vencido (RF-018, Simulator Panel §131.4).
    // Ruta faltante detectada al construir el Simulator Panel — TurbineManager.CheckOverdueMaintenance ya existía sin endpoint.
    [HttpPost("CheckOverdueMaintenance")]
    public IActionResult CheckOverdueMaintenanceEndpoint()
    {
        _turbineManager.CheckOverdueMaintenance();
        return Ok(new { message = "Verificación de mantenimiento vencido ejecutada." });
    }
}
