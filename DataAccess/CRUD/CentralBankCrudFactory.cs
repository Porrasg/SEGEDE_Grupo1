using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

/// <summary>
/// CrudFactory para CentralBank → tblCentralBank (§12.14). Singleton (Id=1).
/// </summary>
public class CentralBankCrudFactory : CrudFactory
{
    public override void Create(BaseDTO baseDTO) =>
        throw new NotSupportedException("Create is not supported for CentralBank (Singleton).");

    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Use specific update methods for CentralBank.");

    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for CentralBank (Singleton).");

    public override T RetrieveById<T>(int id) =>
        throw new NotSupportedException("Use RetrieveSingleton for CentralBank.");

    public override List<T> RetrieveAll<T>() =>
        throw new NotSupportedException("Use RetrieveSingleton for CentralBank.");

    // --- Custom methods ---

    public CentralBank? RetrieveSingleton()
    {
        var op = new Operation { ProcedureName = "RET_SINGLETON_CENT_BANK_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildCentralBank(results[0]) : null;
    }

    public void UpdateInventory(decimal currentInventory, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_INVENTORY_CENT_BANK_PR" };
        op.AddDecimalParameter("@CurrentInventory", currentInventory);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    public void UpdateAutomaticCapacity(decimal automaticCapacity, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_AUTO_CAP_CENT_BANK_PR" };
        op.AddDecimalParameter("@AutomaticCapacity", automaticCapacity);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    public void UpdateManualCapacity(decimal? manualCapacity, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_MANUAL_CAP_CENT_BANK_PR" };
        op.AddNullableDecimalParameter("@ManualCapacity", manualCapacity);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    private static CentralBank BuildCentralBank(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        CurrentInventory = (decimal)row["CurrentInventory"],
        ManualCapacity = row["ManualCapacity"] as decimal?,
        AutomaticCapacity = (decimal)row["AutomaticCapacity"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
