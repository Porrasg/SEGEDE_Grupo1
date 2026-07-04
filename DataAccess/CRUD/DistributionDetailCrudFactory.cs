using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

/// <summary>
/// CrudFactory para DistributionDetail → tblDistributionDetail (§12.18).
/// </summary>
public class DistributionDetailCrudFactory : CrudFactory
{
    public override void Create(BaseDTO baseDTO)
    {
        var d = (DistributionDetail)baseDTO;
        var op = new Operation { ProcedureName = "CRE_DIST_DTL_PR" };
        op.AddIntParameter("@DistributionId", d.DistributionId);
        op.AddIntParameter("@BuyerId", d.BuyerId);
        op.AddIntParameter("@ForecastId", d.ForecastId);
        op.AddDecimalParameter("@RequestedMWh", d.RequestedMWh);
        op.AddDecimalParameter("@AssignedMWh", d.AssignedMWh);
        op.AddDecimalParameter("@UnsuppliedDemand", d.UnsuppliedDemand);
        op.AddDateTimeParameter("@Created", d.Created);
        sqlDao.ExecuteProcedure(op);
    }

    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Update is not supported for DistributionDetail.");

    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for DistributionDetail.");

    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_DIST_DTL_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildDetail(results[0]) : default!;
    }

    public override List<T> RetrieveAll<T>() =>
        throw new NotSupportedException("RetrieveAll is not supported for DistributionDetail.");

    // --- Custom methods ---

    public List<DistributionDetail> RetrieveByDistribution(int distributionId)
    {
        var op = new Operation { ProcedureName = "RET_BY_DIST_DIST_DTL_PR" };
        op.AddIntParameter("@DistributionId", distributionId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildDetail).ToList();
    }

    public List<DistributionDetail> RetrieveByBuyer(int buyerId)
    {
        var op = new Operation { ProcedureName = "RET_BY_BUYER_DIST_DTL_PR" };
        op.AddIntParameter("@BuyerId", buyerId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildDetail).ToList();
    }

    private static DistributionDetail BuildDetail(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        DistributionId = (int)row["DistributionId"],
        BuyerId = (int)row["BuyerId"],
        ForecastId = (int)row["ForecastId"],
        RequestedMWh = (decimal)row["RequestedMWh"],
        AssignedMWh = (decimal)row["AssignedMWh"],
        UnsuppliedDemand = (decimal)row["UnsuppliedDemand"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
