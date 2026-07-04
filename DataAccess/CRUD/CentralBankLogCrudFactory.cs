using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

/// <summary>
/// CrudFactory para CentralBankLog → tblCentralBankLog (§12.15).
/// </summary>
public class CentralBankLogCrudFactory : CrudFactory
{
    public override void Create(BaseDTO baseDTO)
    {
        var l = (CentralBankLog)baseDTO;
        var op = new Operation { ProcedureName = "CRE_CB_LOG_PR" };
        op.AddStringParameter("@MovementType", l.MovementType);
        op.AddDecimalParameter("@Amount", l.Amount);
        op.AddDecimalParameter("@ResultingInventory", l.ResultingInventory);
        op.AddNullableIntParameter("@FlushId", l.FlushId);
        op.AddNullableIntParameter("@DistributionId", l.DistributionId);
        op.AddDateTimeParameter("@EventDate", l.EventDate);
        op.AddDateTimeParameter("@Created", l.Created);
        sqlDao.ExecuteProcedure(op);
    }

    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Update is not supported for CentralBankLog.");

    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for CentralBankLog.");

    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_CB_LOG_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildLog(results[0]) : default!;
    }

    public override List<T> RetrieveAll<T>()
    {
        var op = new Operation { ProcedureName = "RET_ALL_CB_LOG_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(r => (T)(object)BuildLog(r)).ToList();
    }

    // --- Custom methods ---

    public List<CentralBankLog> RetrievePaged(int pageNumber, int pageSize)
    {
        var op = new Operation { ProcedureName = "RET_PAGED_CB_LOG_PR" };
        op.AddIntParameter("@PageNumber", pageNumber);
        op.AddIntParameter("@PageSize", pageSize);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildLog).ToList();
    }

    public List<CentralBankLog> RetrieveByFlush(int flushId)
    {
        var op = new Operation { ProcedureName = "RET_BY_FLUSH_CB_LOG_PR" };
        op.AddIntParameter("@FlushId", flushId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildLog).ToList();
    }

    public List<CentralBankLog> RetrieveByDistribution(int distributionId)
    {
        var op = new Operation { ProcedureName = "RET_BY_DIST_CB_LOG_PR" };
        op.AddIntParameter("@DistributionId", distributionId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildLog).ToList();
    }

    private static CentralBankLog BuildLog(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        MovementType = (string)row["MovementType"],
        Amount = (decimal)row["Amount"],
        ResultingInventory = (decimal)row["ResultingInventory"],
        FlushId = row["FlushId"] as int?,
        DistributionId = row["DistributionId"] as int?,
        EventDate = (DateTime)row["EventDate"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
