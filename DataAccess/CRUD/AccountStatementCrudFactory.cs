using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

/// <summary>
/// CrudFactory para AccountStatement → tblAccountStatement (§12.21). WORM parcial.
/// </summary>
public class AccountStatementCrudFactory : CrudFactory
{
    public override void Create(BaseDTO baseDTO)
    {
        var s = (AccountStatement)baseDTO;
        var op = new Operation { ProcedureName = "CRE_ACCT_STMT_PR" };
        op.AddIntParameter("@BuyerId", s.BuyerId);
        op.AddIntParameter("@DistributionId", s.DistributionId);
        op.AddIntParameter("@ForecastId", s.ForecastId);
        op.AddIntParameter("@Month", s.Month);
        op.AddIntParameter("@Year", s.Year);
        op.AddDecimalParameter("@AssignedMWh", s.AssignedMWh);
        op.AddDecimalParameter("@UnitPrice", s.UnitPrice);
        op.AddDecimalParameter("@TaxPercentage", s.TaxPercentage);
        op.AddDecimalParameter("@Subtotal", s.Subtotal);
        op.AddDecimalParameter("@TaxAmount", s.TaxAmount);
        op.AddDecimalParameter("@Total", s.Total);
        op.AddStringParameter("@Status", s.Status);
        op.AddIntParameter("@RevisionNumber", s.RevisionNumber);
        op.AddNullableIntParameter("@ParentId", s.ParentId);
        op.AddStringParameter("@AnnulmentReason", s.AnnulmentReason);
        op.AddDateTimeParameter("@IssueDate", s.IssueDate);
        op.AddDateTimeParameter("@Created", s.Created);
        sqlDao.ExecuteProcedure(op);
    }

    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Use UpdateAnnulment method instead (WORM parcial).");

    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for AccountStatement (WORM parcial).");

    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_ACCT_STMT_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildStatement(results[0]) : default!;
    }

    public override List<T> RetrieveAll<T>()
    {
        var op = new Operation { ProcedureName = "RET_ALL_ACCT_STMT_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(r => (T)(object)BuildStatement(r)).ToList();
    }

    // --- Custom methods ---

    public List<AccountStatement> RetrieveByBuyer(int buyerId)
    {
        var op = new Operation { ProcedureName = "RET_BY_BUYER_ACCT_STMT_PR" };
        op.AddIntParameter("@BuyerId", buyerId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildStatement).ToList();
    }

    public List<AccountStatement> RetrieveByDistribution(int distributionId)
    {
        var op = new Operation { ProcedureName = "RET_BY_DIST_ACCT_STMT_PR" };
        op.AddIntParameter("@DistributionId", distributionId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildStatement).ToList();
    }

    public void UpdateAnnulment(int id, string status, string annulmentReason, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_ANNUL_ACCT_STMT_PR" };
        op.AddIntParameter("@Id", id);
        op.AddStringParameter("@Status", status);
        op.AddStringParameter("@AnnulmentReason", annulmentReason);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    public AccountStatement? RetrieveCurrentVersion(int buyerId, int month, int year)
    {
        var op = new Operation { ProcedureName = "RET_CURRENT_VERSION_ACCT_STMT_PR" };
        op.AddIntParameter("@BuyerId", buyerId);
        op.AddIntParameter("@Month", month);
        op.AddIntParameter("@Year", year);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildStatement(results[0]) : null;
    }

    private static AccountStatement BuildStatement(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        BuyerId = (int)row["BuyerId"],
        DistributionId = (int)row["DistributionId"],
        ForecastId = (int)row["ForecastId"],
        Month = (int)row["Month"],
        Year = (int)row["Year"],
        AssignedMWh = (decimal)row["AssignedMWh"],
        UnitPrice = (decimal)row["UnitPrice"],
        TaxPercentage = (decimal)row["TaxPercentage"],
        Subtotal = (decimal)row["Subtotal"],
        TaxAmount = (decimal)row["TaxAmount"],
        Total = (decimal)row["Total"],
        Status = (string)row["Status"],
        RevisionNumber = (int)row["RevisionNumber"],
        ParentId = row["ParentId"] as int?,
        AnnulmentReason = row["AnnulmentReason"] as string,
        IssueDate = (DateTime)row["IssueDate"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
