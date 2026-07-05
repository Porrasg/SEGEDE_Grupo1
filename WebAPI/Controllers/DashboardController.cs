using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp.Managers;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST que genera las métricas y KPIs específicos para los paneles de Administrador, Operaciones y Comprador (§14.13).
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DashboardManager _dashboardManager = new();

    // Función de consulta que calcula y retorna los KPIs globales e inventarios para el panel de Administrador.
    [HttpGet("Admin")]
    public IActionResult GetAdminDashboard()
    {
        var d = _dashboardManager.GetAdminDashboard();
        return Ok(new ApiResponse<DashboardAdminResponse> { Success = true, Data = d });
    }

    // Función de consulta que retorna las métricas de turbinas y estado de mantenimiento para el panel del Ingeniero de Operaciones.
    [HttpGet("Operations")]
    public IActionResult GetOperationsDashboard()
    {
        var d = _dashboardManager.GetOperationsDashboard();
        return Ok(new ApiResponse<DashboardOperationsResponse> { Success = true, Data = d });
    }

    // Función de consulta que retorna el consumo histórico, pronósticos vigentes y facturas del panel de Comprador.
    [HttpGet("Buyer")]
    public IActionResult GetBuyerDashboard([FromQuery] int buyerUserId)
    {
        var d = _dashboardManager.GetBuyerDashboard(buyerUserId);
        return Ok(new ApiResponse<DashboardBuyerResponse> { Success = true, Data = d });
    }

    // Endpoint para sembrar o restablecer datos ficticios realistas para pruebas de los dashboards en todos los perfiles.
    [HttpPost("SeedAllTestData")]
    public IActionResult SeedAllTestData()
    {
        new SeederManager().SeedAllDevData();
        return Ok(new ApiResponse<string> { Success = true, Data = "Siembra de datos funcionales completada exitosamente para todos los perfiles." });
    }
}
