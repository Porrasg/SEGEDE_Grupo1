using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.CoreApp.Exceptions;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST que genera las métricas y KPIs específicos para los paneles de Administrador, Operaciones y Comprador (§14.13).
[ApiController]
[Route("api/[controller]")]
public class DashboardController : SgdeControllerBase
{
    private readonly DashboardManager _dashboardManager = new();
    private readonly IWebHostEnvironment _env;

    public DashboardController(IWebHostEnvironment env)
    {
        _env = env;
    }

    // Función de consulta que calcula y retorna los KPIs globales e inventarios para el panel de Administrador. Solo Admin.
    [Authorize(Roles = "Administrator")]
    [HttpGet("Admin")]
    public IActionResult GetAdminDashboard()
    {
        var d = _dashboardManager.GetAdminDashboard();
        return Ok(d);
    }

    // Función de consulta que retorna las métricas de turbinas y estado de mantenimiento para el panel del Ingeniero de Operaciones.
    [Authorize(Roles = "Administrator,Engineer")]
    [HttpGet("Operations")]
    public IActionResult GetOperationsDashboard()
    {
        var d = _dashboardManager.GetOperationsDashboard();
        return Ok(d);
    }

    // Función de consulta que retorna el consumo histórico, pronósticos vigentes y facturas del panel de Comprador. Ownership: propio.
    [Authorize(Roles = "Buyer")]
    [HttpGet("Buyer")]
    public IActionResult GetBuyerDashboard()
    {
        var d = _dashboardManager.GetBuyerDashboard(CallerUserId);
        return Ok(d);
    }

    // Endpoint para sembrar o restablecer datos ficticios realistas para pruebas de los dashboards en todos los perfiles. Solo en desarrollo.
    [AllowAnonymous]
    [HttpPost("SeedAllTestData")]
    public IActionResult SeedAllTestData()
    {
        if (!_env.IsDevelopment())
        {
            throw new UnauthorizedAccessAppException("Endpoint disponible únicamente en entorno de desarrollo.");
        }
        new SeederManager().SeedAllDevData();
        return Ok(new { message = "Siembra de datos funcionales completada exitosamente para todos los perfiles." });
    }
}
