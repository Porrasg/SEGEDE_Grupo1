using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

/// <summary>
/// CrudFactory para LocalBattery → tblLocalBattery (§12.5).
/// </summary>
public class LocalBatteryCrudFactory : CrudFactory
{
    public override void Create(BaseDTO baseDTO)
    {
        var b = (LocalBattery)baseDTO;
        var op = new Operation { ProcedureName = "CRE_LOCAL_BAT_PR" };
        op.AddIntParameter("@TurbineId", b.TurbineId);
        op.AddDecimalParameter("@StoredEnergy", b.StoredEnergy);
        op.AddDateTimeParameter("@Created", b.Created);
        sqlDao.ExecuteProcedure(op);
    }

    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Use UpdateEnergy method instead.");

    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for LocalBattery.");

    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_LOCAL_BAT_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildBattery(results[0]) : default!;
    }

    public override List<T> RetrieveAll<T>()
    {
        var op = new Operation { ProcedureName = "RET_ALL_LOCAL_BAT_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(r => (T)(object)BuildBattery(r)).ToList();
    }

    // --- Custom methods ---

    public LocalBattery? RetrieveByTurbine(int turbineId)
    {
        var op = new Operation { ProcedureName = "RET_BY_TURBINE_LOCAL_BAT_PR" };
        op.AddIntParameter("@TurbineId", turbineId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildBattery(results[0]) : null;
    }

    public void UpdateEnergy(int id, decimal storedEnergy, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_ENERGY_LOCAL_BAT_PR" };
        op.AddIntParameter("@Id", id);
        op.AddDecimalParameter("@StoredEnergy", storedEnergy);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    public List<LocalBattery> RetrieveAllNonEmpty()
    {
        var op = new Operation { ProcedureName = "RET_ALL_NONEMPTY_LOCAL_BAT_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildBattery).ToList();
    }

    private static LocalBattery BuildBattery(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        TurbineId = (int)row["TurbineId"],
        StoredEnergy = (decimal)row["StoredEnergy"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
