using System.Net;
using System.Text.Json;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

namespace SEGEDE_Grupo1.WebAPI.Middleware;

// Middleware global de manejo de excepciones para Web API REST.
// Intercepta errores en el pipeline HTTP y transforma las excepciones de negocio de Capa 0 en respuestas JSON estandarizadas.
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    // Constructor que inicializa el middleware en el pipeline de peticiones HTTP.
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    // Función operativa que ejecuta la intercepción de la petición y captura cualquier excepción no manejada.
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción capturada en la ejecución del endpoint de Web API.");
            await HandleExceptionAsync(context, ex);
        }
    }

    // Manejador interno que evalúa el tipo de excepción y retorna el código de estado HTTP y estructura ApiResponse correspondiente.
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Success = false
        };

        switch (exception)
        {
            case BusinessException be:
                context.Response.StatusCode = IsConflictCode(be.Code)
                    ? (int)HttpStatusCode.Conflict
                    : (int)HttpStatusCode.BadRequest;
                response.Message = be.Message;
                response.ErrorCode = be.Code ?? "BUSINESS_ERROR";
                break;

            case NotFoundException nfe:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = nfe.Message;
                response.ErrorCode = "NOT_FOUND";
                break;

            case UnauthorizedAccessAppException uae:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Message = uae.Message;
                response.ErrorCode = "UNAUTHORIZED_ACCESS";
                break;

            case ValidationException ve:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Error de validación en los datos ingresados.";
                response.ErrorCode = "VALIDATION_ERROR";
                response.Data = ve.Errors;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "Ha ocurrido un error interno en el procesamiento del servidor.";
                response.ErrorCode = "INTERNAL_SERVER_ERROR";
                // In Development, include exception details to help debugging
                if (_env != null && _env.IsDevelopment())
                {
                    response.Data = new { Exception = exception.Message, StackTrace = exception.StackTrace };
                }
                break;
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }

    // Códigos de negocio que representan un conflicto de estado/recurso y deben mapear a HTTP 409.
    private static readonly HashSet<string> ConflictCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "OTP_SERVICE_UNAVAILABLE",
        "FLUSH_IN_PROGRESS",
        "DISTRIBUTION_ALREADY_EXECUTED",
        "ALREADY_ANNULLED"
    };

    // Determina si un código de error representa un conflicto de recursos (HTTP 409),
    // por lista explícita o por convención de nombre (EXISTS/CONFLICT/DUPLICATE).
    private static bool IsConflictCode(string? code)
    {
        if (string.IsNullOrEmpty(code)) return false;
        return ConflictCodes.Contains(code) ||
               code.Contains("EXISTS", StringComparison.OrdinalIgnoreCase) ||
               code.Contains("CONFLICT", StringComparison.OrdinalIgnoreCase) ||
               code.Contains("DUPLICATE", StringComparison.OrdinalIgnoreCase);
    }
}

// Extensión de automatización para registrar el Middleware en el pipeline de configuración HTTP.
public static class ExceptionHandlingMiddlewareExtensions
{
    // Función de configuración que incorpora el middleware de manejo de errores a la aplicación.
    public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
