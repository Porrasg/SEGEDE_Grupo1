$root = "C:\Users\yorze\OneDrive\Documentos\GitHub\SEGEDE_Grupo1"

# Función de automatización y despliegue del entorno en PowerShell.
function New-SkeletonFile($path, $content) {
    $fullPath = Join-Path $root $path
    $dir = Split-Path $fullPath -Parent
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
    Set-Content -Path $fullPath -Value $content.Trim() -Encoding UTF8 -Force
    Write-Host "Creado: $path"
}

# 1. Entities-DTOs
New-SkeletonFile "Entities-DTOs\BaseDTO.cs" @"
namespace SEGEDE_Grupo1.EntitiesDTOs;

// TODO: Implementar propiedades base (Id, Created, Updated) según documento técnico §3.1.
public abstract class BaseDTO
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}
"@

New-SkeletonFile "Entities-DTOs\Constants\SystemActor.cs" @"
namespace SEGEDE_Grupo1.EntitiesDTOs.Constants;

// TODO: Actor del sistema unificado según documento técnico §3.2.
public static class SystemActor
{
    public const int Id = 0;
    public const string Name = "System";
}
"@

New-SkeletonFile "Entities-DTOs\Constants\UserRoles.cs" @"
namespace SEGEDE_Grupo1.EntitiesDTOs.Constants;

// TODO: Definir roles de usuario (Administrator, Engineer, Buyer) según documento técnico §4.
public static class UserRoles
{
    public const string Administrator = "Administrator";
    public const string Engineer = "Engineer";
    public const string Buyer = "Buyer";
}
"@

New-SkeletonFile "Entities-DTOs\Constants\UserStates.cs" @"
namespace SEGEDE_Grupo1.EntitiesDTOs.Constants;

// TODO: Definir estados de usuario (PendingActivation, Active, Blocked, Inactive) según documento técnico §4.
public static class UserStates
{
    public const string PendingActivation = "PendingActivation";
    public const string Active = "Active";
    public const string Blocked = "Blocked";
    public const string Inactive = "Inactive";
}
"@

New-SkeletonFile "Entities-DTOs\Entities\User.cs" @"
namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

// TODO: Entidad User mapeada a tblUsers según documento técnico §9.1.
public class User : BaseDTO
{
    public string Identification { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int FailedAttempts { get; set; }
    public DateTime? BlockedAt { get; set; }
}
"@

New-SkeletonFile "Entities-DTOs\Entities\Turbine.cs" @"
namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

// TODO: Entidad Turbine mapeada a tblTurbines según documento técnico §9.3.
public class Turbine : BaseDTO
{
    public string UniqueCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal WeeklyNominalCapacity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastMaintenance { get; set; }
    public DateTime LastStateChange { get; set; }
}
"@

New-SkeletonFile "Entities-DTOs\Entities\Maintenance.cs" @"
namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

// TODO: Entidad Maintenance mapeada a tblMaintenances según documento técnico §9.6.
public class Maintenance : BaseDTO
{
    public int TurbineId { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public DateTime EstimatedStartDate { get; set; }
    public DateTime EstimatedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public string? Result { get; set; }
    public string Status { get; set; } = string.Empty;
}
"@

New-SkeletonFile "Entities-DTOs\DTOs\ApiResponse.cs" @"
namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs;

// TODO: Envoltura estándar para respuestas HTTP de la API según documento técnico §3.3.
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public string[]? Errors { get; set; }
    public string? ErrorCode { get; set; }
}
"@

New-SkeletonFile "Entities-DTOs\DTOs\PagedRequest.cs" @"
namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs;

// TODO: Estructura base para solicitudes paginadas según documento técnico §3.4.
public class PagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
"@

New-SkeletonFile "Entities-DTOs\DTOs\PagedResponse.cs" @"
namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs;

// TODO: Estructura base para respuestas paginadas según documento técnico §3.4.
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
"@

New-SkeletonFile "Entities-DTOs\Exceptions\BusinessException.cs" @"
namespace SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

// TODO: Excepción de negocio base según documento técnico §5.
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}
"@

New-SkeletonFile "Entities-DTOs\Validation\ValidationResult.cs" @"
namespace SEGEDE_Grupo1.EntitiesDTOs.Validation;

// TODO: Resultado de validación para lógica de negocio y entidades según documento técnico §6.
public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
}
"@

New-SkeletonFile "Entities-DTOs\Helpers\TimeHelper.cs" @"
namespace SEGEDE_Grupo1.EntitiesDTOs.Helpers;

// TODO: Helper de tiempo con zona horaria America/Costa_Rica según documento técnico §7.1.
public static class TimeHelper
{
    public static DateTime NowCR()
    {
        return DateTime.UtcNow;
    }
}
"@

# 2. DataAccess
New-SkeletonFile "DataAccess\DAO\SqlDao.cs" @"
namespace SEGEDE_Grupo1.DataAccess.DAO;

// TODO: DAO para ejecución de Stored Procedures usando Microsoft.Data.SqlClient según documento técnico §11.1.
public class SqlDao
{
    // TODO: Implementar ExecuteNonQuery, ExecuteReader, ExecuteScalar y gestión de conexión a Azure SQL.
}
"@

New-SkeletonFile "DataAccess\DAO\Operation.cs" @"
namespace SEGEDE_Grupo1.DataAccess.DAO;

// TODO: Clase para encapsular parámetros y nombres de Stored Procedures según documento técnico §11.2.
public class Operation
{
    public string ProcedureName { get; set; } = string.Empty;
}
"@

New-SkeletonFile "DataAccess\CRUD\CrudFactory.cs" @"
namespace SEGEDE_Grupo1.DataAccess.CRUD;

// TODO: Clase base/interfaz para todas las CrudFactories según documento técnico §12.
public abstract class CrudFactory<T>
{
}
"@

New-SkeletonFile "DataAccess\CRUD\UserCrudFactory.cs" @"
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// TODO: CrudFactory para la entidad User utilizando Stored Procedures según §12.1.
public class UserCrudFactory : CrudFactory<User>
{
}
"@

New-SkeletonFile "DataAccess\CRUD\TurbineCrudFactory.cs" @"
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// TODO: CrudFactory para la entidad Turbine utilizando Stored Procedures según §12.3.
public class TurbineCrudFactory : CrudFactory<Turbine>
{
}
"@

New-SkeletonFile "DataAccess\CRUD\MaintenanceCrudFactory.cs" @"
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// TODO: CrudFactory para la entidad Maintenance utilizando Stored Procedures según §12.6.
public class MaintenanceCrudFactory : CrudFactory<Maintenance>
{
}
"@

# 3. CoreApp
New-SkeletonFile "CoreApp\Helpers\JwtHelper.cs" @"
namespace SEGEDE_Grupo1.CoreApp.Helpers;

// TODO: Generación y validación de tokens JWT según documento técnico §13.1.
public static class JwtHelper
{
}
"@

New-SkeletonFile "CoreApp\Helpers\PasswordHasher.cs" @"
namespace SEGEDE_Grupo1.CoreApp.Helpers;

// TODO: Hasheo y verificación de contraseñas con BCrypt según documento técnico §13.2 (work factor 12).
public static class PasswordHasher
{
}
"@

New-SkeletonFile "CoreApp\External\OtpServiceClient.cs" @"
namespace SEGEDE_Grupo1.CoreApp.External;

// TODO: Cliente para API externa de terceros de OTP según documento técnico §13.3 y §19.
public class OtpServiceClient
{
}
"@

New-SkeletonFile "CoreApp\Export\CsvBuilder.cs" @"
namespace SEGEDE_Grupo1.CoreApp.Export;

// TODO: Constructor de archivos CSV para exportación de datos y estados de cuenta según §20.1.
public class CsvBuilder
{
}
"@

New-SkeletonFile "CoreApp\Export\ExcelBuilder.cs" @"
namespace SEGEDE_Grupo1.CoreApp.Export;

// TODO: Constructor de archivos Excel para exportaciones según §20.1.
public class ExcelBuilder
{
}
"@

New-SkeletonFile "CoreApp\Export\HtmlStatementBuilder.cs" @"
namespace SEGEDE_Grupo1.CoreApp.Export;

// TODO: Constructor HTML para visualización e impresión de estados de cuenta según §20.1.
public class HtmlStatementBuilder
{
}
"@

New-SkeletonFile "CoreApp\Managers\UserManager.cs" @"
using SEGEDE_Grupo1.DataAccess.CRUD;

namespace SEGEDE_Grupo1.CoreApp.Managers;

// TODO: Manager de usuarios según documento técnico §14.1. Instanciación directa sin IoC.
public class UserManager
{
    private readonly UserCrudFactory _userCrudFactory = new();
}
"@

New-SkeletonFile "CoreApp\Managers\TurbineManager.cs" @"
using SEGEDE_Grupo1.DataAccess.CRUD;

namespace SEGEDE_Grupo1.CoreApp.Managers;

// TODO: Manager de turbinas según documento técnico §14.2.
public class TurbineManager
{
    private readonly TurbineCrudFactory _turbineCrudFactory = new();
}
"@

New-SkeletonFile "CoreApp\Managers\DashboardManager.cs" @"
namespace SEGEDE_Grupo1.CoreApp.Managers;

// TODO: Manager para KPIs y paneles de control (Dashboard) según documento técnico §14.13.
public class DashboardManager
{
}
"@

# 4. WebAPI
New-SkeletonFile "WebAPI\Controllers\UsersController.cs" @"
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
"@

New-SkeletonFile "WebAPI\Controllers\TurbinesController.cs" @"
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
"@

New-SkeletonFile "WebAPI\Controllers\DashboardController.cs" @"
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
"@

New-SkeletonFile "WebAPI\Middleware\ExceptionHandlingMiddleware.cs" @"
namespace SEGEDE_Grupo1.WebAPI.Middleware;

// TODO: Middleware global de manejo de excepciones para formatear respuestas como ApiResponse según §15.2.
public class ExceptionHandlingMiddleware
{
}
"@

New-SkeletonFile "WebAPI\BackgroundServices\JobBase.cs" @"
namespace SEGEDE_Grupo1.WebAPI.BackgroundServices;

// TODO: Clase base abstracta para trabajos en segundo plano (Jobs automáticos) según documento técnico §17.
public abstract class JobBase : BackgroundService
{
}
"@

New-SkeletonFile "WebAPI\BackgroundServices\EnergySimulationJob.cs" @"
namespace SEGEDE_Grupo1.WebAPI.BackgroundServices;

// TODO: Trabajo automático en segundo plano para simular generación de energía cada 30 segundos según §17.3.
public class EnergySimulationJob : JobBase
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
"@

# 5. WebApp
New-SkeletonFile "WebApp\wwwroot\js\apiClient.js" @"
// TODO: Cliente HTTP modular para comunicación AJAX con la Web API (§24.1).
const ApiClient = {
    baseUrl: "https://localhost:7001/api",
    get: async function(endpoint) { console.log("TODO: GET " + endpoint); },
    post: async function(endpoint, data) { console.log("TODO: POST " + endpoint, data); }
};
"@

New-SkeletonFile "WebApp\wwwroot\js\session.js" @"
// TODO: Módulo de gestión de sesión y JWT en el navegador (§24.2).
const Session = {
    getToken: function() { return null; },
    setToken: function(token) { console.log("TODO: Token almacenado"); },
    clear: function() { }
};
"@

New-SkeletonFile "WebApp\wwwroot\js\pages-controller\LoginViewController.js" @"
// TODO: Controlador de eventos y comportamiento DOM para la página Login (§22.1, §23).
document.addEventListener("DOMContentLoaded", function() {
    console.log("TODO: Inicializar LoginViewController.");
});
"@

New-SkeletonFile "WebApp\wwwroot\js\pages-controller\AdminDashboardViewController.js" @"
// TODO: Controlador JS para el panel principal de Administración (§22.1, §27).
document.addEventListener("DOMContentLoaded", function() {
    console.log("TODO: Inicializar AdminDashboardViewController.");
});
"@

New-SkeletonFile "WebApp\Pages\Login.cshtml" @"
@page
@model SEGEDE_Grupo1.WebApp.Pages.LoginModel
@{
    ViewData["Title"] = "Login";
}

<div class="container mt-5">
    <div class="card max-w-md mx-auto">
        <div class="card-body">
            <h3 class="card-title text-center">@ViewData["Title"]</h3>
            <p class="text-muted text-center">Esqueleto inicial de inicio de sesión.</p>
        </div>
    </div>
</div>

@section Scripts {
    <script src="~/js/pages-controller/LoginViewController.js"></script>
}
"@

New-SkeletonFile "WebApp\Pages\Login.cshtml.cs" @"
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SEGEDE_Grupo1.WebApp.Pages;

// TODO: PageModel para Login (LoginStep1 y LoginStep2) consumindo la API REST (§23).
public class LoginModel : PageModel
{
    public void OnGet() { }
}
"@

New-SkeletonFile "WebApp\Pages\Admin\Dashboard.cshtml" @"
@page
@model SEGEDE_Grupo1.WebApp.Pages.Admin.DashboardModel
@{
    ViewData["Title"] = "Admin Dashboard";
}

<div class="container mt-4">
    <h2>@ViewData["Title"]</h2>
    <p class="text-muted">Esqueleto del panel de control de Administración.</p>
</div>

@section Scripts {
    <script src="~/js/pages-controller/AdminDashboardViewController.js"></script>
}
"@

New-SkeletonFile "WebApp\Pages\Admin\Dashboard.cshtml.cs" @"
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SEGEDE_Grupo1.WebApp.Pages.Admin;

// TODO: PageModel para Admin Dashboard consumiendo /api/Dashboard/Admin (§27).
public class DashboardModel : PageModel
{
    public void OnGet() { }
}
"@

New-SkeletonFile "WebApp\Pages\Engineer\Dashboard.cshtml" @"
@page
@model SEGEDE_Grupo1.WebApp.Pages.Engineer.DashboardModel
@{
    ViewData["Title"] = "Engineer Dashboard";
}

<div class="container mt-4">
    <h2>@ViewData["Title"]</h2>
    <p class="text-muted">Esqueleto del panel de operaciones de Ingeniería.</p>
</div>
"@

New-SkeletonFile "WebApp\Pages\Engineer\Dashboard.cshtml.cs" @"
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SEGEDE_Grupo1.WebApp.Pages.Engineer;

// TODO: PageModel para Engineer Dashboard consumiendo /api/Dashboard/Operations (§27).
public class DashboardModel : PageModel
{
    public void OnGet() { }
}
"@

New-SkeletonFile "WebApp\Pages\Buyer\Dashboard.cshtml" @"
@page
@model SEGEDE_Grupo1.WebApp.Pages.Buyer.DashboardModel
@{
    ViewData["Title"] = "Buyer Dashboard";
}

<div class="container mt-4">
    <h2>@ViewData["Title"]</h2>
    <p class="text-muted">Esqueleto del panel de gestión de Compras y Consumo.</p>
</div>
"@

New-SkeletonFile "WebApp\Pages\Buyer\Dashboard.cshtml.cs" @"
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SEGEDE_Grupo1.WebApp.Pages.Buyer;

// TODO: PageModel para Buyer Dashboard consumiendo /api/Dashboard/Buyer (§27).
public class DashboardModel : PageModel
{
    public void OnGet() { }
}
"@
Write-Host "Esqueleto generado con éxito en todas las capas."
