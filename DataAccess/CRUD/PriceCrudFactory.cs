using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para Price → tblPrice (§12.19).
public class PriceCrudFactory : CrudFactory
{
    public override void Create(BaseDTO baseDTO)
    {
        var p = (Price)baseDTO;
        var op = new Operation { ProcedureName = "CRE_PRICE_PR" };
        op.AddDecimalParameter("@PriceCRCPerMWh", p.PriceCRCPerMWh);
        op.AddDateTimeParameter("@ValidFrom", p.ValidFrom);
        op.AddNullableDateTimeParameter("@ValidTo", p.ValidTo);
        op.AddBoolParameter("@IsActive", p.IsActive);
        op.AddDateTimeParameter("@Created", p.Created);
        sqlDao.ExecuteProcedure(op);
    }

    // Invoca el SP de modificación para actualizar los campos operacionales del registro en la base de datos.
    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Update is not supported for Price.");

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for Price.");

    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_PRICE_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildPrice(results[0]) : default!;
    }

    public override List<T> RetrieveAll<T>()
    {
        var op = new Operation { ProcedureName = "RET_ALL_PRICE_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(r => (T)(object)BuildPrice(r)).ToList();
    }

    // --- Custom methods ---

    public Price? RetrieveActive()
    {
        var op = new Operation { ProcedureName = "RET_ACTIVE_PRICE_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildPrice(results[0]) : null;
    }

    public Price? RetrieveAtDateTime(DateTime timestamp)
    {
        var op = new Operation { ProcedureName = "RET_AT_DATETIME_PRICE_PR" };
        op.AddDateTimeParameter("@Timestamp", timestamp);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildPrice(results[0]) : null;
    }

    private static Price BuildPrice(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        PriceCRCPerMWh = (decimal)row["PriceCRCPerMWh"],
        ValidFrom = (DateTime)row["ValidFrom"],
        ValidTo = row["ValidTo"] as DateTime?,
        IsActive = (bool)row["IsActive"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
