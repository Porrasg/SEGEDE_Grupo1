using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp.Managers;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;
using SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para la gestión de usuarios, registro, autenticación en dos pasos y ciclo de vida de cuentas (§14.1).
[ApiController]
[Route("api/[controller]")]
public class UsersController : SgdeControllerBase
{
    private readonly UserManager _userManager = new();
    private readonly IWebHostEnvironment _env;

    public UsersController(IWebHostEnvironment env)
    {
        _env = env;
    }

    // Método manejador que procesa el registro de un nuevo comprador recibiendo sus datos vía HTTP POST.
    [AllowAnonymous]
    [HttpPost("Register")]
    public IActionResult Register([FromBody] RegisterBuyerRequest request)
    {
        _userManager.Register(request);
        return Ok(new ApiResponse<object> { Success = true, Message = "Comprador registrado y OTP de activación enviado con éxito." });
    }

    // Método manejador que ejecuta el primer paso de inicio de sesión validando credenciales y enviando código OTP.
    [AllowAnonymous]
    [HttpPost("LoginStep1")]
    public IActionResult LoginStep1([FromBody] LoginStep1Request request)
    {
        _userManager.LoginStep1(request);
        return Ok(new ApiResponse<object> { Success = true, Message = "Paso 1 completado. Código OTP enviado a su correo." });
    }

    // Método manejador que verifica el código OTP del segundo paso y retorna el token JWT de acceso.
    [AllowAnonymous]
    [HttpPost("LoginStep2")]
    public IActionResult LoginStep2([FromBody] LoginStep2Request request)
    {
        var response = _userManager.LoginStep2(request);
        return Ok(new ApiResponse<LoginResponse> { Success = true, Message = "Inicio de sesión exitoso.", Data = response });
    }

    // Método manejador que procesa la activación de una cuenta recién creada mediante código de verificación OTP.
    [AllowAnonymous]
    [HttpPost("Activate")]
    public IActionResult Activate([FromBody] ActivateAccountRequest request)
    {
        _userManager.Activate(request);
        return Ok(new ApiResponse<object> { Success = true, Message = "Cuenta activada correctamente." });
    }

    // Método manejador que inicia el proceso de recuperación de contraseña enviando un código de resguardo.
    [AllowAnonymous]
    [HttpPost("RecoverPassword")]
    public IActionResult RecoverPassword([FromBody] RecoverPasswordRequest request)
    {
        _userManager.RecoverPassword(request);
        return Ok(new ApiResponse<object> { Success = true, Message = "Código de recuperación enviado a su correo electrónico." });
    }

    // Método manejador que restablece la contraseña del usuario tras validar el código OTP de recuperación.
    [AllowAnonymous]
    [HttpPost("ResetPassword")]
    public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
    {
        _userManager.ResetPassword(request);
        return Ok(new ApiResponse<object> { Success = true, Message = "Contraseña restablecida con éxito." });
    }

    // Función de consulta que busca y retorna el perfil público de un usuario por su identificador primario.
    // Ownership (§31): Admin/Engineer ven cualquier perfil; Buyer solo el propio.
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        if (CallerRole != "Administrator" && CallerRole != "Engineer")
        {
            RequireOwnershipOrAdmin(id);
        }
        var user = _userManager.RetrieveById(id);
        return Ok(new ApiResponse<UserSafeResponse> { Success = true, Data = user });
    }

    // Función de consulta que recupera el listado paginado de usuarios activos en el sistema.
    [Authorize(Roles = "Administrator")]
    [HttpGet("RetrieveAll")]
    public IActionResult RetrieveAll()
    {
        var result = _userManager.RetrieveAll();
        return Ok(new ApiResponse<List<UserSafeResponse>> { Success = true, Data = result });
    }

    // Método manejador que reenvía un código OTP (§14.1). Anónimo: se usa antes de tener sesión (login/activación/recuperación).
    [AllowAnonymous]
    [HttpPost("ResendOtp")]
    public IActionResult ResendOtp([FromBody] ResendOtpRequest request)
    {
        _userManager.ResendOtp(request);
        return Ok(new ApiResponse<object> { Success = true, Message = "Código OTP reenviado con éxito." });
    }

    // Método manejador para creación de usuarios internos (Engineer/Admin) por un Administrador (§14.1).
    [Authorize(Roles = "Administrator")]
    [HttpPost("Internal")]
    public IActionResult CreateInternal([FromBody] CreateInternalUserRequest request)
    {
        _userManager.CreateInternal(request);
        return Ok(new ApiResponse<object> { Success = true, Message = "Usuario interno creado con éxito." });
    }

    // Endpoint de conveniencia para sembrar/reiniciar usuarios de prueba (Admin, Engineer, Buyer). Solo en entorno de desarrollo.
    [AllowAnonymous]
    [HttpPost("SeedDev")]
    public IActionResult SeedDev()
    {
        if (!_env.IsDevelopment())
        {
            throw new UnauthorizedAccessAppException("Endpoint disponible únicamente en entorno de desarrollo.");
        }
        _userManager.SeedDevUsers();
        return Ok(new ApiResponse<object> { Success = true, Message = "Usuarios de prueba creados/activados correctamente." });
    }

    // Método manejador que actualiza datos administrativos de un usuario (§14.1).
    [Authorize(Roles = "Administrator")]
    [HttpPut("Update")]
    public IActionResult UpdateUser([FromBody] UpdateUserRequest request)
    {
        _userManager.UpdateUser(request);
        return Ok(new ApiResponse<object> { Success = true, Message = "Usuario actualizado correctamente." });
    }

    // Método manejador para que el Buyer autenticado edite su propio perfil (RF-010, §14.1). Ownership implícito: siempre opera sobre CallerUserId.
    [Authorize(Roles = "Administrator,Engineer,Buyer")]
    [HttpPut("UpdateProfile")]
    public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        _userManager.UpdateProfile(request, CallerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Perfil actualizado correctamente." });
    }

    // Método manejador para borrado lógico / desactivación de un usuario (§14.1).
    // Buyer permitido: el manager valida ownership (solo puede desactivar su propia cuenta).
    [Authorize(Roles = "Administrator,Buyer")]
    [HttpPost("Deactivate")]
    public IActionResult Deactivate([FromBody] DeactivateUserRequest request)
    {
        _userManager.Deactivate(request, CallerUserId, CallerRole);
        return Ok(new ApiResponse<object> { Success = true, Message = "Usuario desactivado correctamente." });
    }

    // Método manejador para reactivación de usuarios inactivos por el Administrador (§14.1).
    [Authorize(Roles = "Administrator")]
    [HttpPost("Reactivate/{id:int}")]
    public IActionResult Reactivate(int id)
    {
        _userManager.Reactivate(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Usuario reactivado correctamente." });
    }
}
