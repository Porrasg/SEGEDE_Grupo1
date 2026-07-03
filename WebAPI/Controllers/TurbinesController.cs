using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp.Managers;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TurbinesController : ControllerBase
{
    private readonly TurbineManager _turbineManager = new();

    [HttpGet("RetrieveAll")]
    public IActionResult RetrieveAll()
    {
        return Ok(new ApiResponse<object> { Success = true, Message = "Endpoint esqueleto configurado." });
    }
}
