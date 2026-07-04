using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp.Managers;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para consultar la bitácora inmutable WORM y gestionar el archivo en frío de auditoría (§14.12).
[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly AuditManager _auditManager = new();

    // Función de consulta que retorna la bitácora paginada de eventos de seguridad por módulo.
    [HttpGet("ByModule")]
    public IActionResult GetByModule([FromQuery] string module, [FromQuery] string callerRole = "Administrator", [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = _auditManager.RetrieveByModule(module, callerRole, new PagedRequest { Page = page, PageSize = pageSize });
        return Ok(new ApiResponse<PagedResponse<AuditLog>> { Success = true, Data = result });
    }

    // Método manejador que archiva en frío los registros de auditoría antiguos respetando el modelo WORM.
    [HttpPost("ArchiveColdRecords")]
    public IActionResult ArchiveColdRecords()
    {
        _auditManager.ArchiveColdRecords();
        return Ok(new ApiResponse<object> { Success = true, Message = "Proceso de archivado en frío completado con éxito." });
    }
}
