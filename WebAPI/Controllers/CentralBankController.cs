using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para consultar el inventario global del Banco Central y bitácora de saturación (§14.7).
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,Engineer")]
public class CentralBankController : SgdeControllerBase
{
    private readonly CentralBankManager _centralBankManager = new();

    // Función de consulta que retorna el nivel de inventario actual disponible en el Banco Central de Energía.
    [HttpGet("Inventory")]
    public IActionResult GetInventory()
    {
        var inv = _centralBankManager.Retrieve();
        return Ok(new ApiResponse<CentralBank> { Success = true, Data = inv });
    }

    // Función de consulta que lista la bitácora paginada de entradas, salidas y eventos de saturación del Banco Central.
    [HttpGet("MovementLogs")]
    public IActionResult GetMovementLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = _centralBankManager.RetrieveLogs(new PagedRequest { Page = page, PageSize = pageSize });
        return Ok(new ApiResponse<PagedResponse<CentralBankLog>> { Success = true, Data = result });
    }

    // Método manejador que fija manualmente la capacidad efectiva del Banco Central (CB-01/CB-02). Solo Admin.
    // Ruta faltante detectada en la auditoría de v2 §53 — CentralBankManager.SetManualCapacity ya existía sin endpoint que lo expusiera.
    [Authorize(Roles = "Administrator")]
    [HttpPut("ManualCapacity")]
    public IActionResult SetManualCapacity([FromBody] SetManualCapacityRequest request)
    {
        _centralBankManager.SetManualCapacity(request, CallerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Capacidad manual del Banco Central actualizada." });
    }
}
