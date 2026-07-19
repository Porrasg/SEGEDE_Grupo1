using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SEGEDE_Grupo1.WebAPI.Middleware;

// Filtro global que envuelve todas las respuestas 200 OK en un sobre estándar { data, message }
// para que el cliente JavaScript pueda acceder siempre a res?.data y res?.message de forma uniforme.
public class ApiResponseWrapperFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is not OkObjectResult okResult || okResult.Value is null)
            return;

        var value = okResult.Value;
        var type = value.GetType();

        // Si ya está envuelto (tiene propiedad "data" o "Data"), no volver a envolver.
        if (type.GetProperty("data") != null || type.GetProperty("Data") != null)
            return;

        // Extraer el mensaje del objeto original si tiene una propiedad "message"/"Message".
        var messageProp = type.GetProperty("message") ?? type.GetProperty("Message");
        var message = messageProp?.GetValue(value)?.ToString() ?? "OK";

        okResult.Value = new { data = value, message };
    }
}
