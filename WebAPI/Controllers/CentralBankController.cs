using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para consultar el inventario global del Banco Central y bitácora de saturación (§14.7).
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,Engineer")]
public class CentralBankController : SgdeControllerBase
{
    private readonly CentralBankManager _centralBankManager = new();

    [HttpGet("Inventory")]
    public IActionResult GetInventory()
    {
        var inv = _centralBankManager.Retrieve();
        return Ok(inv);
    }

    [HttpGet("MovementLogs")]
    public IActionResult GetMovementLogs()
    {
        var result = _centralBankManager.RetrieveLogs();
        return Ok(result);
    }

    [Authorize(Roles = "Administrator")]
    [HttpPut("ManualCapacity")]
    public IActionResult SetManualCapacity([FromBody] SetManualCapacityRequest request)
    {
        _centralBankManager.SetManualCapacity(request, CallerUserId);
        return Ok(new { message = "Capacidad manual del Banco Central actualizada." });
    }
}
