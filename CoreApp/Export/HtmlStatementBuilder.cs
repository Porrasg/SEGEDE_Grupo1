using System.Text;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.CoreApp.Export;

// Constructor HTML para visualización e impresión de estados de cuenta según §20.1.
// Produce HTML imprimible diseñado para que el frontend invoque "Guardar como PDF".
public class HtmlStatementBuilder
{
    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public byte[] BuildStatementHtml(AccountStatement s, User? buyer)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head><meta charset=\"utf-8\"><title>Account Statement</title>");
        sb.AppendLine("<style>body{font-family:Arial,sans-serif;margin:40px;} table{width:100%;border-collapse:collapse;margin-top:20px;} th,td{padding:10px;border:1px solid #ccc;text-align:left;} th{background-color:#f4f4f4;}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine($"<h1>Account Statement #{s.Id}</h1>");
        if (buyer != null)
        {
            sb.AppendLine($"<p><strong>Buyer:</strong> {buyer.FirstName} {buyer.LastName} ({buyer.Email})</p>");
        }
        sb.AppendLine($"<p><strong>Period:</strong> {s.Month}/{s.Year} | <strong>Issue Date:</strong> {s.IssueDate:yyyy-MM-dd}</p>");
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>Description</th><th>Value</th></tr>");
        sb.AppendLine($"<tr><td>Assigned Energy</td><td>{s.AssignedMWh:F4} MWh</td></tr>");
        sb.AppendLine($"<tr><td>Unit Price</td><td>{s.UnitPrice:C}</td></tr>");
        sb.AppendLine($"<tr><td>Subtotal</td><td>{s.Subtotal:C}</td></tr>");
        sb.AppendLine($"<tr><td>Tax Rate ({s.TaxPercentage:P2})</td><td>{s.TaxAmount:C}</td></tr>");
        sb.AppendLine($"<tr><th>Total Amount</th><th>{s.Total:C}</th></tr>");
        sb.AppendLine("</table>");
        sb.AppendLine($"<p style=\"margin-top:20px;\"><strong>Status:</strong> {s.Status} | <strong>Revision:</strong> {s.RevisionNumber}</p>");
        sb.AppendLine("</body></html>");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
