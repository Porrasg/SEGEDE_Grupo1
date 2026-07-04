using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp.Managers;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para consultar el inventario global del Banco Central y bitácora de saturación (§14.7).
[ApiController]
[Route("api/[controller]")]
public class CentralBankController : ControllerBase
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
}
