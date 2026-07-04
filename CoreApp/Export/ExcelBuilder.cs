using System.Text;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.CoreApp.Export;

// Constructor de archivos Excel para exportaciones según §20.1.
// Utiliza el formato XML Spreadsheet 2003 nativo (sin librerías externas).
public class ExcelBuilder
{
    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public byte[] BuildStatementExcel(AccountStatement s, User? buyer)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\"?>");
        sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
        sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
        sb.AppendLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
        sb.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
        sb.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
        sb.AppendLine(" <Worksheet ss:Name=\"AccountStatement\">");
        sb.AppendLine("  <Table>");
        sb.AppendLine($"   <Row><Cell><Data ss:Type=\"String\">Account Statement #{s.Id}</Data></Cell></Row>");
        if (buyer != null)
        {
            sb.AppendLine($"   <Row><Cell><Data ss:Type=\"String\">Buyer</Data></Cell><Cell><Data ss:Type=\"String\">{buyer.FirstName} {buyer.LastName}</Data></Cell></Row>");
        }
        sb.AppendLine($"   <Row><Cell><Data ss:Type=\"String\">Period</Data></Cell><Cell><Data ss:Type=\"String\">{s.Month}/{s.Year}</Data></Cell></Row>");
        sb.AppendLine($"   <Row><Cell><Data ss:Type=\"String\">Assigned Energy (MWh)</Data></Cell><Cell><Data ss:Type=\"Number\">{s.AssignedMWh:F4}</Data></Cell></Row>");
        sb.AppendLine($"   <Row><Cell><Data ss:Type=\"String\">Unit Price (CRC)</Data></Cell><Cell><Data ss:Type=\"Number\">{s.UnitPrice:F2}</Data></Cell></Row>");
        sb.AppendLine($"   <Row><Cell><Data ss:Type=\"String\">Subtotal (CRC)</Data></Cell><Cell><Data ss:Type=\"Number\">{s.Subtotal:F2}</Data></Cell></Row>");
        sb.AppendLine($"   <Row><Cell><Data ss:Type=\"String\">Tax Rate</Data></Cell><Cell><Data ss:Type=\"String\">{s.TaxPercentage:P2}</Data></Cell></Row>");
        sb.AppendLine($"   <Row><Cell><Data ss:Type=\"String\">Tax Amount (CRC)</Data></Cell><Cell><Data ss:Type=\"Number\">{s.TaxAmount:F2}</Data></Cell></Row>");
        sb.AppendLine($"   <Row><Cell><Data ss:Type=\"String\">Total Amount (CRC)</Data></Cell><Cell><Data ss:Type=\"Number\">{s.Total:F2}</Data></Cell></Row>");
        sb.AppendLine($"   <Row><Cell><Data ss:Type=\"String\">Status</Data></Cell><Cell><Data ss:Type=\"String\">{s.Status}</Data></Cell></Row>");
        sb.AppendLine($"   <Row><Cell><Data ss:Type=\"String\">Revision Number</Data></Cell><Cell><Data ss:Type=\"Number\">{s.RevisionNumber}</Data></Cell></Row>");
        sb.AppendLine("  </Table>");
        sb.AppendLine(" </Worksheet>");
        sb.AppendLine("</Workbook>");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
