using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para consultar la bitácora inmutable WORM y gestionar el archivo en frío de auditoría (§14.12).
// Engineer accede pero el módulo Billing queda excluido internamente por RN-030 (AuditManager usa el callerRole real).
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,Engineer")]
public class AuditController : SgdeControllerBase
{
    private readonly AuditManager _auditManager = new();

    // Función de consulta que retorna la bitácora paginada de eventos de seguridad por módulo.
    [HttpGet("ByModule")]
    public IActionResult GetByModule([FromQuery] string module, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = _auditManager.RetrieveByModule(module, CallerRole, new PagedRequest { Page = page, PageSize = pageSize });
        return Ok(new ApiResponse<PagedResponse<AuditLog>> { Success = true, Data = result });
    }

    // Función de consulta que retorna la bitácora paginada de eventos por usuario.
    [HttpGet("ByUser")]
    public IActionResult GetByUser([FromQuery] int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = _auditManager.RetrieveByUser(userId, new PagedRequest { Page = page, PageSize = pageSize });
        return Ok(new ApiResponse<PagedResponse<AuditLog>> { Success = true, Data = result });
    }

    // Función de consulta que retorna la bitácora en un rango de fechas con filtrado RN-030.
    [HttpGet("ByDateRange")]
    public IActionResult GetByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = _auditManager.RetrieveByDateRange(from, to, CallerRole, new PagedRequest { Page = page, PageSize = pageSize });
        return Ok(new ApiResponse<PagedResponse<AuditLog>> { Success = true, Data = result });
    }

    // Método manejador que archiva en frío los registros de auditoría antiguos respetando el modelo WORM. Solo Admin.
    [Authorize(Roles = "Administrator")]
    [HttpPost("ArchiveColdRecords")]
    public IActionResult ArchiveColdRecords()
    {
        _auditManager.ArchiveColdRecords();
        return Ok(new ApiResponse<object> { Success = true, Message = "Proceso de archivado en frío completado con éxito." });
    }
}
