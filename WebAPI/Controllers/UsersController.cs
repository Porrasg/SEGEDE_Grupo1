using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp.Managers;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserManager _userManager = new();

    [HttpPost("Register")]
    public IActionResult Register()
    {
        return Ok(new ApiResponse<object> { Success = true, Message = "Endpoint esqueleto configurado." });
    }
}
