using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para la ejecución de flushes y traslados ACID desde baterías locales al Banco Central (§14.6).
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,Engineer")]
public class FlushController : SgdeControllerBase
{
    private readonly FlushManager _flushManager = new();

    [Authorize(Roles = "Administrator")]
    [HttpPost("ExecuteManual")]
    public IActionResult ExecuteManual()
    {
        _flushManager.ExecuteManualFlush(CallerUserId);
        return Ok(new { message = "Flush transaccional ejecutado hacia el Banco Central." });
    }

    [HttpGet("Config")]
    public IActionResult GetConfig()
    {
        var c = _flushManager.GetFlushConfig();
        return Ok(c);
    }

    [Authorize(Roles = "Administrator")]
    [HttpPut("Config")]
    public IActionResult UpdateConfig([FromBody] UpdateFlushConfigRequest request)
    {
        _flushManager.UpdateFlushConfig(request, CallerUserId);
        return Ok(new { message = "Configuración de flush actualizada con éxito." });
    }

    [HttpGet("History")]
    public IActionResult GetHistory()
    {
        var result = _flushManager.RetrieveFlushHistory();
        return Ok(result);
    }
}
