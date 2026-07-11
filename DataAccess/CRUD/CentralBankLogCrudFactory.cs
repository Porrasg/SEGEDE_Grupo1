using Microsoft.Data.SqlClient;
using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para CentralBankLog → tblCentralBankLog (§12.15).
public class CentralBankLogCrudFactory : CrudFactory
{
    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
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

    // Invoca el SP de modificación para actualizar los campos operacionales del registro en la base de datos.
    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Update is not supported for CentralBankLog.");

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for CentralBankLog.");

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_CB_LOG_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildLog(results[0]) : default!;
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override List<T> RetrieveAll<T>()
    {
        var op = new Operation { ProcedureName = "RET_ALL_CB_LOG_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(r => (T)(object)BuildLog(r)).ToList();
    }

    // --- Overload transaccional (§37.25) ---

    public void Create(BaseDTO baseDTO, SqlConnection conn, SqlTransaction tx)
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
        sqlDao.ExecuteProcedureInTransaction(op, conn, tx);
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

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public List<CentralBankLog> RetrieveByFlush(int flushId)
    {
        var op = new Operation { ProcedureName = "RET_BY_FLUSH_CB_LOG_PR" };
        op.AddIntParameter("@FlushId", flushId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildLog).ToList();
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public List<CentralBankLog> RetrieveByDistribution(int distributionId)
    {
        var op = new Operation { ProcedureName = "RET_BY_DIST_CB_LOG_PR" };
        op.AddIntParameter("@DistributionId", distributionId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildLog).ToList();
    }

    // Función que registra eventos de trazabilidad y seguridad en la bitácora inmutable del sistema (WORM).
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
