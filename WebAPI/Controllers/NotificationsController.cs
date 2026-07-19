using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para el monitoreo de la cola asíncrona de correos y reintentos con backoff exponencial (§14.11).
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : SgdeControllerBase
{
    private readonly NotificationManager _notificationManager = new();

    [Authorize(Roles = "Administrator")]
    [HttpPost("ProcessQueue")]
    public IActionResult ProcessQueue()
    {
        _notificationManager.ProcessQueue();
        return Ok(new { message = "Procesamiento de la cola de notificaciones finalizado." });
    }

    [HttpGet("ByUser/{userId:int}")]
    public IActionResult GetByUser(int userId)
    {
        RequireOwnershipOrAdmin(userId);
        var result = _notificationManager.RetrieveByUser(userId);
        return Ok(result);
    }
}
