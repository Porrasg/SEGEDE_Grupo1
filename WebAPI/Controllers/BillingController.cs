using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEGEDE_Grupo1.CoreApp;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.WebAPI.Controllers;

// Controlador REST para la fijación de precios, impuestos, facturación y exportación de estados de cuenta (§14.10).
// Dominio exclusivo de Administrator y Buyer (propio) — Engineer no tiene acceso a Billing (SEC-08).
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,Buyer")]
public class BillingController : SgdeControllerBase
{
    private readonly BillingManager _billingManager = new();

    // Método manejador que establece el nuevo precio por MWh cerrando la vigencia del precio anterior. Solo Admin.
    [Authorize(Roles = "Administrator")]
    [HttpPost("SetPrice")]
    public IActionResult SetPrice([FromBody] SetPriceRequest request)
    {
        _billingManager.SetPrice(request, CallerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Nuevo precio por MWh fijado en el sistema." });
    }

    // Función de consulta que retorna el historial de precios (Admin/Prices). Ruta faltante detectada en la auditoría de v2 §53.
    [Authorize(Roles = "Administrator")]
    [HttpGet("PriceHistory")]
    public IActionResult GetPriceHistory()
    {
        var result = _billingManager.RetrievePriceHistory();
        return Ok(new ApiResponse<List<Price>> { Success = true, Data = result });
    }

    // Método manejador que actualiza el porcentaje de impuesto aplicable en la facturación mensual. Solo Admin.
    [Authorize(Roles = "Administrator")]
    [HttpPost("SetTax")]
    public IActionResult SetTax([FromBody] SetTaxRequest request)
    {
        _billingManager.SetTax(request, CallerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Porcentaje de impuesto actualizado con éxito." });
    }

    // Función de consulta que retorna el historial de impuestos (Admin/Taxes). Ruta faltante detectada en la auditoría de v2 §53.
    [Authorize(Roles = "Administrator")]
    [HttpGet("TaxHistory")]
    public IActionResult GetTaxHistory()
    {
        var result = _billingManager.RetrieveTaxHistory();
        return Ok(new ApiResponse<List<Tax>> { Success = true, Data = result });
    }

    // Función de consulta que recupera el listado de estados de cuenta emitidos para un comprador o globalmente. Ownership: Buyer solo ve los propios.
    [HttpGet("Statements")]
    public IActionResult GetStatements([FromQuery] int? buyerId = null)
    {
        var result = _billingManager.RetrieveStatements(buyerId, CallerUserId, CallerRole);
        return Ok(new ApiResponse<List<AccountStatement>> { Success = true, Data = result });
    }

    // Método manejador que procesa la anulación justificada de un estado de cuenta emitido. Solo Admin.
    [Authorize(Roles = "Administrator")]
    [HttpPost("AnnulStatement")]
    public IActionResult AnnulStatement([FromBody] AnnulStatementRequest request)
    {
        _billingManager.AnnulStatement(request, CallerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Estado de cuenta anulado formalmente." });
    }

    // Método manejador que regenera una nueva revisión de un estado de cuenta anulado. Solo Admin.
    // Ruta faltante detectada en la auditoría de v2 §53 — BillingManager.RegenerateStatement ya existía sin endpoint que lo expusiera.
    [Authorize(Roles = "Administrator")]
    [HttpPost("RegenerateStatement")]
    public IActionResult RegenerateStatement([FromBody] RegenerateStatementRequest request)
    {
        _billingManager.RegenerateStatement(request, CallerUserId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Estado de cuenta regenerado con nueva revisión." });
    }

    // Función de consulta que retorna la bitácora completa de exportaciones (Admin/Exports). Solo Admin.
    // Ruta faltante detectada al wireear Admin/Exports.cshtml — BillingManager no tenía forma de listar ExportLog.
    [Authorize(Roles = "Administrator")]
    [HttpGet("ExportLogs")]
    public IActionResult GetExportLogs()
    {
        var result = _billingManager.RetrieveExportLogs();
        return Ok(new ApiResponse<List<ExportLog>> { Success = true, Data = result });
    }

    // Método manejador que exporta y descarga un estado de cuenta en el formato solicitado (CSV, Excel o HTML/PDF) (§14.10, §20.1). Ownership: Buyer solo exporta lo propio (BIL-04).
    [HttpPost("Export")]
    public IActionResult Export([FromBody] ExportStatementRequest request)
    {
        var fileBytes = _billingManager.ExportStatement(request, CallerUserId, CallerRole);
        string contentType = request.Format.ToUpper() switch
        {
            "CSV" => "text/csv",
            "EXCEL" or "XLSX" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "text/html"
        };
        string fileName = $"EstadoCuenta_{request.StatementId}_{request.Format.ToLower()}.{(request.Format.ToUpper() == "EXCEL" ? "xlsx" : request.Format.ToLower())}";
        return File(fileBytes, contentType, fileName);
    }
}
