using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

/// <summary>
/// CrudFactory para Tax → tblTax (§12.20).
/// </summary>
public class TaxCrudFactory : CrudFactory
{
    public override void Create(BaseDTO baseDTO)
    {
        var t = (Tax)baseDTO;
        var op = new Operation { ProcedureName = "CRE_TAX_PR" };
        op.AddStringParameter("@Name", t.Name);
        op.AddDecimalParameter("@Percentage", t.Percentage);
        op.AddDateTimeParameter("@ValidFrom", t.ValidFrom);
        op.AddNullableDateTimeParameter("@ValidTo", t.ValidTo);
        op.AddBoolParameter("@IsActive", t.IsActive);
        op.AddDateTimeParameter("@Created", t.Created);
        sqlDao.ExecuteProcedure(op);
    }

    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Update is not supported for Tax.");

    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for Tax.");

    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_TAX_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildTax(results[0]) : default!;
    }

    public override List<T> RetrieveAll<T>()
    {
        var op = new Operation { ProcedureName = "RET_ALL_TAX_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(r => (T)(object)BuildTax(r)).ToList();
    }

    // --- Custom methods ---

    public Tax? RetrieveActive()
    {
        var op = new Operation { ProcedureName = "RET_ACTIVE_TAX_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildTax(results[0]) : null;
    }

    public Tax? RetrieveAtDateTime(DateTime timestamp)
    {
        var op = new Operation { ProcedureName = "RET_AT_DATETIME_TAX_PR" };
        op.AddDateTimeParameter("@Timestamp", timestamp);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildTax(results[0]) : null;
    }

    private static Tax BuildTax(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        Name = (string)row["Name"],
        Percentage = (decimal)row["Percentage"],
        ValidFrom = (DateTime)row["ValidFrom"],
        ValidTo = row["ValidTo"] as DateTime?,
        IsActive = (bool)row["IsActive"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
