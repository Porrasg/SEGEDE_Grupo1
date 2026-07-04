using System.Text;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.CoreApp.Export;

/// <summary>
/// Constructor de archivos CSV para exportación de datos y estados de cuenta según §20.1.
/// Utiliza StringBuilder, UTF-8 con BOM y formateo adecuado.
/// </summary>
public class CsvBuilder
{
    public byte[] BuildStatementCsv(AccountStatement s, User? buyer)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Account Statement CSV Export");
        sb.AppendLine($"Statement ID,{s.Id}");
        sb.AppendLine($"Issue Date,{s.IssueDate:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Buyer ID,{s.BuyerId}");
        if (buyer != null)
        {
            sb.AppendLine($"Buyer Name,\"{buyer.FirstName} {buyer.LastName}\"");
            sb.AppendLine($"Buyer Email,\"{buyer.Email}\"");
        }
        sb.AppendLine($"Period,{s.Month}/{s.Year}");
        sb.AppendLine($"Assigned Energy (MWh),{s.AssignedMWh:F4}");
        sb.AppendLine($"Unit Price (CRC),{s.UnitPrice:F2}");
        sb.AppendLine($"Subtotal (CRC),{s.Subtotal:F2}");
        sb.AppendLine($"Tax Rate,{s.TaxPercentage:P2}");
        sb.AppendLine($"Tax Amount (CRC),{s.TaxAmount:F2}");
        sb.AppendLine($"Total Amount (CRC),{s.Total:F2}");
        sb.AppendLine($"Status,{s.Status}");
        sb.AppendLine($"Revision Number,{s.RevisionNumber}");

        byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
        byte[] withBom = new byte[bytes.Length + 3];
        withBom[0] = 0xEF;
        withBom[1] = 0xBB;
        withBom[2] = 0xBF;
        Array.Copy(bytes, 0, withBom, 3, bytes.Length);
        return withBom;
    }
}
