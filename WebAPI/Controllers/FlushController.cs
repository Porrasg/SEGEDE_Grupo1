using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp.Managers;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para la ejecución de flushes y traslados ACID desde baterías locales al Banco Central (§14.6).
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,Engineer")]
public class FlushController : SgdeControllerBase
{
    private readonly FlushManager _flushManager = new();

    // Método manejador que ejecuta de forma transaccional un flush manual trasladando energía al Banco Central. Solo Admin.
    [Authorize(Roles = "Administrator")]
    [HttpPost("ExecuteManual")]
    public IActionResult ExecuteManual()
    {
        _flushManager.ExecuteManualFlush(CallerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Flush transaccional ejecutado hacia el Banco Central." });
    }

    // Función de consulta que obtiene la configuración actual de flushes automáticos y umbrales de disparo.
    [HttpGet("Config")]
    public IActionResult GetConfig()
    {
        var c = _flushManager.GetFlushConfig();
        return Ok(new ApiResponse<FlushConfig> { Success = true, Data = c });
    }

    // Método manejador que actualiza la configuración horaria y de frecuencia para la ejecución de flushes. Solo Admin.
    [Authorize(Roles = "Administrator")]
    [HttpPut("Config")]
    public IActionResult UpdateConfig([FromBody] UpdateFlushConfigRequest request)
    {
        _flushManager.UpdateFlushConfig(request, CallerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Configuración de flush actualizada con éxito." });
    }

    // Función de consulta que retorna el historial paginado de flushes ejecutados en el sistema.
    [HttpGet("History")]
    public IActionResult GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = _flushManager.RetrieveFlushHistory(new PagedRequest { Page = page, PageSize = pageSize });
        return Ok(new ApiResponse<PagedResponse<Flush>> { Success = true, Data = result });
    }
}
