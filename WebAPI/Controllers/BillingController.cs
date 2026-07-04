using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp.Managers;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para la fijación de precios, impuestos, facturación y exportación de estados de cuenta (§14.10).
[ApiController]
[Route("api/[controller]")]
public class BillingController : ControllerBase
{
    private readonly BillingManager _billingManager = new();

    // Método manejador que establece el nuevo precio por MWh cerrando la vigencia del precio anterior.
    [HttpPost("SetPrice")]
    public IActionResult SetPrice([FromBody] SetPriceRequest request, [FromQuery] int callerUserId = 1)
    {
        _billingManager.SetPrice(request, callerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Nuevo precio por MWh fijado en el sistema." });
    }

    // Método manejador que actualiza el porcentaje de impuesto aplicable en la facturación mensual.
    [HttpPost("SetTax")]
    public IActionResult SetTax([FromBody] SetTaxRequest request, [FromQuery] int callerUserId = 1)
    {
        _billingManager.SetTax(request, callerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Porcentaje de impuesto actualizado con éxito." });
    }

    // Función de consulta que recupera el listado de estados de cuenta emitidos para un comprador o globalmente.
    [HttpGet("Statements")]
    public IActionResult GetStatements([FromQuery] int? buyerId = null, [FromQuery] int callerUserId = 1, [FromQuery] string callerRole = "Administrator")
    {
        var result = _billingManager.RetrieveStatements(buyerId, callerUserId, callerRole);
        return Ok(new ApiResponse<List<AccountStatement>> { Success = true, Data = result });
    }

    // Método manejador que procesa la anulación justificada de un estado de cuenta emitido.
    [HttpPost("AnnulStatement")]
    public IActionResult AnnulStatement([FromBody] AnnulStatementRequest request, [FromQuery] int callerUserId = 1)
    {
        _billingManager.AnnulStatement(request, callerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Estado de cuenta anulado formalmente." });
    }
}
