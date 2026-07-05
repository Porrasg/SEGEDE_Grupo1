using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp.Managers;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para la gestión de usuarios, registro, autenticación en dos pasos y ciclo de vida de cuentas (§14.1).
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserManager _userManager = new();

    // Método manejador que procesa el registro de un nuevo comprador recibiendo sus datos vía HTTP POST.
    [HttpPost("Register")]
    public IActionResult Register([FromBody] RegisterBuyerRequest request)
    {
        _userManager.Register(request);
        return Ok(new ApiResponse<object> { Success = true, Message = "Comprador registrado y OTP de activación enviado con éxito." });
    }

    // Método manejador que ejecuta el primer paso de inicio de sesión validando credenciales y enviando código OTP.
    [HttpPost("LoginStep1")]
    public IActionResult LoginStep1([FromBody] LoginStep1Request request)
    {
        _userManager.LoginStep1(request);
        return Ok(new ApiResponse<object> { Success = true, Message = "Paso 1 completado. Código OTP enviado a su correo." });
    }

    // Método manejador que verifica el código OTP del segundo paso y retorna el token JWT de acceso.
    [HttpPost("LoginStep2")]
    public IActionResult LoginStep2([FromBody] LoginStep2Request request)
    {
        var response = _userManager.LoginStep2(request);
        return Ok(new ApiResponse<LoginResponse> { Success = true, Message = "Inicio de sesión exitoso.", Data = response });
    }

    // Método manejador que procesa la activación de una cuenta recién creada mediante código de verificación OTP.
    [HttpPost("Activate")]
    public IActionResult Activate([FromBody] ActivateAccountRequest request)
    {
        _userManager.Activate(request);
        return Ok(new ApiResponse<object> { Success = true, Message = "Cuenta activada correctamente." });
    }

    // Método manejador que inicia el proceso de recuperación de contraseña enviando un código de resguardo.
    [HttpPost("RecoverPassword")]
    public IActionResult RecoverPassword([FromBody] RecoverPasswordRequest request)
    {
        _userManager.RecoverPassword(request);
        return Ok(new ApiResponse<object> { Success = true, Message = "Código de recuperación enviado a su correo electrónico." });
    }

    // Método manejador que restablece la contraseña del usuario tras validar el código OTP de recuperación.
    [HttpPost("ResetPassword")]
    public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
    {
        _userManager.ResetPassword(request);
        return Ok(new ApiResponse<object> { Success = true, Message = "Contraseña restablecida con éxito." });
    }

    // Función de consulta que busca y retorna el perfil público de un usuario por su identificador primario.
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var user = _userManager.RetrieveById(id);
        return Ok(new ApiResponse<UserSafeResponse> { Success = true, Data = user });
    }

    // Función de consulta que recupera el listado paginado de usuarios activos en el sistema.
    [HttpGet("RetrieveAll")]
    public IActionResult RetrieveAll()
    {
        var result = _userManager.RetrieveAll();
        return Ok(new ApiResponse<List<UserSafeResponse>> { Success = true, Data = result });
    }

    // Método manejador que reenvía un código OTP (§14.1).
    [HttpPost("ResendOtp")]
    public IActionResult ResendOtp([FromBody] ResendOtpRequest request)
    {
        _userManager.ResendOtp(request);
        return Ok(new ApiResponse<object> { Success = true, Message = "Código OTP reenviado con éxito." });
    }

    // Método manejador para creación de usuarios internos (Engineer/Admin) por un Administrador (§14.1).
    [HttpPost("Internal")]
    public IActionResult CreateInternal([FromBody] CreateInternalUserRequest request)
    {
        _userManager.CreateInternal(request);
        return Ok(new ApiResponse<object> { Success = true, Message = "Usuario interno creado con éxito." });
    }

    // Endpoint de conveniencia para sembrar/reiniciar usuarios de prueba (Admin, Engineer, Buyer) en local.
    [HttpPost("SeedDev")]
    public IActionResult SeedDev()
    {
        _userManager.SeedDevUsers();
        return Ok(new ApiResponse<object> { Success = true, Message = "Usuarios de prueba creados/activados correctamente." });
    }

    // Método manejador que actualiza datos administrativos de un usuario (§14.1).
    [HttpPut("Update")]
    public IActionResult UpdateUser([FromBody] UpdateUserRequest request)
    {
        _userManager.UpdateUser(request);
        return Ok(new ApiResponse<object> { Success = true, Message = "Usuario actualizado correctamente." });
    }

    // Método manejador para borrado lógico / desactivación de un usuario (§14.1).
    [HttpPost("Deactivate")]
    public IActionResult Deactivate([FromBody] DeactivateUserRequest request)
    {
        _userManager.Deactivate(request, 1, "Administrator");
        return Ok(new ApiResponse<object> { Success = true, Message = "Usuario desactivado correctamente." });
    }

    // Método manejador para reactivación de usuarios inactivos por el Administrador (§14.1).
    [HttpPost("Reactivate/{id:int}")]
    public IActionResult Reactivate(int id)
    {
        _userManager.Reactivate(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Usuario reactivado correctamente." });
    }
}
