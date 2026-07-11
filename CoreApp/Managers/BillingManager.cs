using SEGEDE_Grupo1.CoreApp.Export;
using SEGEDE_Grupo1.DataAccess.CRUD;
using SEGEDE_Grupo1.EntitiesDTOs.Constants;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;
using SEGEDE_Grupo1.EntitiesDTOs.Exceptions;
using SEGEDE_Grupo1.EntitiesDTOs.Helpers;
using SEGEDE_Grupo1.EntitiesDTOs.Validation;

namespace SEGEDE_Grupo1.CoreApp.Managers;

// Manager de Facturación y Finanzas (§14.10). Instanciación directa con new sin IoC.
// Gestiona precios, impuestos, consulta, anulación y regeneración de estados de cuenta, así como exportación en formatos CSV, Excel y HTML/PDF.
public class BillingManager
{
    private readonly AccountStatementCrudFactory _statementFactory = new();
    private readonly PriceCrudFactory _priceFactory = new();
    private readonly TaxCrudFactory _taxFactory = new();
    private readonly ExportLogCrudFactory _exportLogFactory = new();
    private readonly UserCrudFactory _userFactory = new();
    private readonly AuditManager _auditManager = new();

    private readonly CsvBuilder _csvBuilder = new();
    private readonly ExcelBuilder _excelBuilder = new();
    private readonly HtmlStatementBuilder _htmlBuilder = new();

    // RF-057: Establecer un nuevo precio por MWh. Cierra el precio activo vigente (manejado por el SP de inserción) y registra el nuevo.
    public void SetPrice(SetPriceRequest r, int callerUserId)
    {
        BillingValidator.ValidatePrice(r.PriceCRCPerMWh).ThrowIfInvalid();

        var now = TimeHelper.NowCR();
        var oldPrice = _priceFactory.RetrieveActive();
        string oldVal = oldPrice != null ? $"{oldPrice.PriceCRCPerMWh:F2}" : "None";

        var price = new Price
        {
            PriceCRCPerMWh = r.PriceCRCPerMWh,
            ValidFrom = now,
            ValidTo = null,
            IsActive = true,
            Created = now
        };

        _priceFactory.Create(price);
        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.Billing, AuditActions.Create, "tblPrice", 0, oldVal, $"{r.PriceCRCPerMWh:F2}");
    }

    // RF-057: Retorna el historial de precios configurados.
    public List<Price> RetrievePriceHistory()
    {
        return _priceFactory.RetrieveAll<Price>();
    }

    // RF-058: Establecer un nuevo impuesto. Valida que sea fracción entre 0 y 1 (ej. 0.13), cierra el anterior y crea el nuevo.
    public void SetTax(SetTaxRequest r, int callerUserId)
    {
        BillingValidator.ValidateTax(r.Percentage).ThrowIfInvalid();

        if (string.IsNullOrWhiteSpace(r.Name))
        {
            throw new BusinessException("Tax name is required.", "INVALID_TAX_NAME");
        }

        var now = TimeHelper.NowCR();
        var oldTax = _taxFactory.RetrieveActive();
        string oldVal = oldTax != null ? $"{oldTax.Name}: {oldTax.Percentage:P2}" : "None";

        var tax = new Tax
        {
            Name = r.Name,
            Percentage = r.Percentage,
            ValidFrom = now,
            ValidTo = null,
            IsActive = true,
            Created = now
        };

        _taxFactory.Create(tax);
        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.Billing, AuditActions.Create, "tblTax", 0, oldVal, $"{r.Name}: {r.Percentage:P2}");
    }

    // RF-058: Retorna el historial de impuestos configurados.
    public List<Tax> RetrieveTaxHistory()
    {
        return _taxFactory.RetrieveAll<Tax>();
    }

    // RF-065/072: Retorna la bitácora completa de exportaciones (evidencia WORM, Admin/Exports en v2 §85).
    public List<ExportLog> RetrieveExportLogs()
    {
        return _exportLogFactory.RetrieveAll<ExportLog>();
    }

    // RF-064: Retorna estados de cuenta. Si buyerId es null, retorna todos (Administrador/Ingeniero); si se especifica buyerId, verifica ownership.
    public List<AccountStatement> RetrieveStatements(int? buyerId, int callerUserId, string callerRole)
    {
        if (buyerId.HasValue)
        {
            if (!string.Equals(callerRole, "Administrator", StringComparison.OrdinalIgnoreCase) && buyerId.Value != callerUserId)
            {
                throw new UnauthorizedAccessAppException("You can only view your own account statements.", "OWNERSHIP_VIOLATION");
            }
            return _statementFactory.RetrieveByBuyer(buyerId.Value);
        }

        if (!string.Equals(callerRole, "Administrator", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(callerRole, "Engineer", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessAppException("Administrator or Engineer role required to view all statements.", "UNAUTHORIZED_ACCESS");
        }

        return _statementFactory.RetrieveAll<AccountStatement>();
    }

    // RF-062: Anula un estado de cuenta. Valida que exista motivo, marca Status=Annulled sin tocar valores financieros.
    public void AnnulStatement(AnnulStatementRequest r, int callerUserId)
    {
        BillingValidator.ValidateAnnulment(r.Reason).ThrowIfInvalid();

        var stmt = _statementFactory.RetrieveById<AccountStatement>(r.StatementId) ?? throw new NotFoundException("Account statement not found.");

        if (string.Equals(stmt.Status, StatementStates.Annulled, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Account statement is already annulled.", "ALREADY_ANNULLED");
        }

        _statementFactory.UpdateAnnulment(stmt.Id, StatementStates.Annulled, r.Reason, TimeHelper.NowCR());
        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.Billing, AuditActions.Update, "tblAccountStatement", stmt.Id, StatementStates.Issued, StatementStates.Annulled);
    }

    // RF-063: Regenera un estado de cuenta anulado (§17.10). Copia valores congelados con RevisionNumber incrementado y ParentId apuntando al origen.
    public void RegenerateStatement(RegenerateStatementRequest r, int callerUserId)
    {
        var orig = _statementFactory.RetrieveById<AccountStatement>(r.OriginalStatementId) ?? throw new NotFoundException("Original account statement not found.");

        if (!string.Equals(orig.Status, StatementStates.Annulled, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Only annulled account statements can be regenerated (RF-063).", "NOT_ANNULLED");
        }

        var now = TimeHelper.NowCR();
        var newStmt = new AccountStatement
        {
            BuyerId = orig.BuyerId,
            DistributionId = orig.DistributionId,
            ForecastId = orig.ForecastId,
            Month = orig.Month,
            Year = orig.Year,
            AssignedMWh = orig.AssignedMWh,
            UnitPrice = orig.UnitPrice,
            TaxPercentage = orig.TaxPercentage,
            Subtotal = orig.Subtotal,
            TaxAmount = orig.TaxAmount,
            Total = orig.Total,
            Status = StatementStates.Issued,
            RevisionNumber = orig.RevisionNumber + 1,
            ParentId = orig.Id,
            AnnulmentReason = null,
            IssueDate = now,
            Created = now
        };

        _statementFactory.Create(newStmt);
        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.Billing, AuditActions.Create, "tblAccountStatement", 0, $"Annulled #{orig.Id} (Rev {orig.RevisionNumber})", $"Regenerated (Rev {newStmt.RevisionNumber})");
    }

    // RF-065/072: Exportación de estado de cuenta (§20.1). Verifica ownership, construye el archivo (CSV, Excel o HTML/PDF) y registra en ExportLog.
    public byte[] ExportStatement(ExportStatementRequest r, int callerUserId, string callerRole)
    {
        var stmt = _statementFactory.RetrieveById<AccountStatement>(r.StatementId) ?? throw new NotFoundException("Account statement not found.");

        if (!string.Equals(callerRole, "Administrator", StringComparison.OrdinalIgnoreCase) && stmt.BuyerId != callerUserId)
        {
            throw new UnauthorizedAccessAppException("You can only export your own account statements.", "OWNERSHIP_VIOLATION");
        }

        var buyer = _userFactory.RetrieveById<User>(stmt.BuyerId);
        byte[] fileBytes;
        string formatName;

        if (string.Equals(r.Format, ExportFormats.CSV, StringComparison.OrdinalIgnoreCase))
        {
            fileBytes = _csvBuilder.BuildStatementCsv(stmt, buyer);
            formatName = ExportFormats.CSV;
        }
        else if (string.Equals(r.Format, ExportFormats.Excel, StringComparison.OrdinalIgnoreCase))
        {
            fileBytes = _excelBuilder.BuildStatementExcel(stmt, buyer);
            formatName = ExportFormats.Excel;
        }
        else if (string.Equals(r.Format, ExportFormats.PDF, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(r.Format, "HTML", StringComparison.OrdinalIgnoreCase))
        {
            fileBytes = _htmlBuilder.BuildStatementHtml(stmt, buyer);
            formatName = ExportFormats.PDF;
        }
        else
        {
            throw new BusinessException($"Unsupported export format: {r.Format}", "UNSUPPORTED_FORMAT");
        }

        var now = TimeHelper.NowCR();
        string clonePath = $"statement_{stmt.Id}_rev{stmt.RevisionNumber}.{formatName.ToLower()}";

        var expLog = new ExportLog
        {
            UserId = callerUserId,
            DocumentType = "AccountStatement",
            DocumentId = stmt.Id,
            Format = formatName,
            CloneFilePath = clonePath,
            EventDate = now,
            Created = now
        };

        _exportLogFactory.Create(expLog);
        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.Billing, AuditActions.Export, "tblAccountStatement", stmt.Id, null, $"Exported as {formatName}");

        return fileBytes;
    }
}
