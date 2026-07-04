using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp.Managers;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para el monitoreo de la cola asíncrona de correos y reintentos con backoff exponencial (§14.11).
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationManager _notificationManager = new();

    // Método manejador que dispara de forma inmediata el procesamiento de notificaciones pendientes en la cola.
    [HttpPost("ProcessQueue")]
    public IActionResult ProcessQueue()
    {
        _notificationManager.ProcessQueue();
        return Ok(new ApiResponse<object> { Success = true, Message = "Procesamiento asíncrono de la cola de notificaciones finalizado." });
    }

    // Función de consulta que lista los correos encolados y su estado actual de envío paginados por usuario.
    [HttpGet("ByUser/{userId:int}")]
    public IActionResult GetByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = _notificationManager.RetrieveByUser(userId, new PagedRequest { Page = page, PageSize = pageSize });
        return Ok(new ApiResponse<PagedResponse<NotificationQueue>> { Success = true, Data = result });
    }
}
