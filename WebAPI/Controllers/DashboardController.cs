using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp.Managers;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DashboardManager _dashboardManager = new();

    [HttpGet("Admin")]
    public IActionResult Admin()
    {
        return Ok(new ApiResponse<object> { Success = true, Message = "Endpoint esqueleto configurado." });
    }
}
