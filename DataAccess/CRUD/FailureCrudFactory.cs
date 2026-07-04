using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para Failure → tblFailures (§12.7).
public class FailureCrudFactory : CrudFactory
{
    public override void Create(BaseDTO baseDTO)
    {
        var f = (Failure)baseDTO;
        var op = new Operation { ProcedureName = "CRE_FAILURE_PR" };
        op.AddIntParameter("@TurbineId", f.TurbineId);
        op.AddDateTimeParameter("@FailureDate", f.FailureDate);
        op.AddStringParameter("@Description", f.Description);
        op.AddStringParameter("@Severity", f.Severity);
        op.AddDateTimeParameter("@Created", f.Created);
        sqlDao.ExecuteProcedure(op);
    }

    // Invoca el SP de modificación para actualizar los campos operacionales del registro en la base de datos.
    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Update is not supported for Failure.");

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for Failure.");

    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_FAILURE_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildFailure(results[0]) : default!;
    }

    public override List<T> RetrieveAll<T>()
    {
        var op = new Operation { ProcedureName = "RET_ALL_FAILURE_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(r => (T)(object)BuildFailure(r)).ToList();
    }

    // --- Custom methods ---

    public List<Failure> RetrieveByTurbine(int turbineId)
    {
        var op = new Operation { ProcedureName = "RET_BY_TURBINE_FAILURE_PR" };
        op.AddIntParameter("@TurbineId", turbineId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildFailure).ToList();
    }

    public int RetrieveCountByTurbine(int turbineId)
    {
        var op = new Operation { ProcedureName = "RET_COUNT_BY_TURBINE_FAILURE_PR" };
        op.AddIntParameter("@TurbineId", turbineId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? Convert.ToInt32(results[0].Values.First()) : 0;
    }

    private static Failure BuildFailure(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        TurbineId = (int)row["TurbineId"],
        FailureDate = (DateTime)row["FailureDate"],
        Description = (string)row["Description"],
        Severity = (string)row["Severity"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
