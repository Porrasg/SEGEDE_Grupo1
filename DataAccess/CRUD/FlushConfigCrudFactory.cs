using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

/// <summary>
/// CrudFactory para FlushConfig → tblFlushConfig (§12.10). Singleton (Id=1).
/// </summary>
public class FlushConfigCrudFactory : CrudFactory
{
    public override void Create(BaseDTO baseDTO) =>
        throw new NotSupportedException("Create is not supported for FlushConfig (Singleton).");

    public override void Update(BaseDTO baseDTO)
    {
        var c = (FlushConfig)baseDTO;
        UpdateSingleton(c.ExecutionTime, c.IsAutomatic, c.Updated);
    }

    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for FlushConfig (Singleton).");

    public override T RetrieveById<T>(int id) =>
        throw new NotSupportedException("Use RetrieveSingleton for FlushConfig.");

    public override List<T> RetrieveAll<T>() =>
        throw new NotSupportedException("Use RetrieveSingleton for FlushConfig.");

    // --- Custom methods ---

    public FlushConfig? RetrieveSingleton()
    {
        var op = new Operation { ProcedureName = "RET_SINGLETON_FLUSH_CFG_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildConfig(results[0]) : null;
    }

    public void UpdateSingleton(TimeSpan executionTime, bool isAutomatic, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_SINGLETON_FLUSH_CFG_PR" };
        op.AddTimeParameter("@ExecutionTime", executionTime);
        op.AddBoolParameter("@IsAutomatic", isAutomatic);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    private static FlushConfig BuildConfig(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        ExecutionTime = (TimeSpan)row["ExecutionTime"],
        IsAutomatic = (bool)row["IsAutomatic"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
