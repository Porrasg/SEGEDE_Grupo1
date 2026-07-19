using Microsoft.Data.SqlClient;
using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para SaturationLog → tblSaturationLog (§12.13). WORM.
public class SaturationLogCrudFactory : CrudFactory
{
    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public override void Create(BaseDTO baseDTO)
    {
        var s = (SaturationLog)baseDTO;
        var op = new Operation { ProcedureName = "CRE_SAT_LOG_PR" };
        op.AddIntParameter("@FlushId", s.FlushId);
        op.AddDecimalParameter("@PreviousInventory", s.PreviousInventory);
        op.AddDecimalParameter("@NewInventory", s.NewInventory);
        op.AddDecimalParameter("@ExcessEnergy", s.ExcessEnergy);
        op.AddDateTimeParameter("@EventDate", s.EventDate);
        op.AddDateTimeParameter("@Created", s.Created);
        sqlDao.ExecuteProcedure(op);
    }

    // Invoca el SP de modificación para actualizar los campos operacionales del registro en la base de datos.
    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Update is not supported for SaturationLog (WORM).");

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for SaturationLog (WORM).");

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_SAT_LOG_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildLog(results[0]) : default!;
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override List<T> RetrieveAll<T>() =>
        throw new NotSupportedException("RetrieveAll is not supported for SaturationLog.");

    // --- Overload transaccional (§37.25) ---

    public void Create(BaseDTO baseDTO, SqlConnection conn, SqlTransaction tx)
    {
        var s = (SaturationLog)baseDTO;
        var op = new Operation { ProcedureName = "CRE_SAT_LOG_PR" };
        op.AddIntParameter("@FlushId", s.FlushId);
        op.AddDecimalParameter("@PreviousInventory", s.PreviousInventory);
        op.AddDecimalParameter("@NewInventory", s.NewInventory);
        op.AddDecimalParameter("@ExcessEnergy", s.ExcessEnergy);
        op.AddDateTimeParameter("@EventDate", s.EventDate);
        op.AddDateTimeParameter("@Created", s.Created);
        sqlDao.ExecuteProcedureInTransaction(op, conn, tx);
    }

    // --- Custom methods ---

    public List<SaturationLog> RetrieveByFlush(int flushId)
    {
        var op = new Operation { ProcedureName = "RET_BY_FLUSH_SAT_LOG_PR" };
        op.AddIntParameter("@FlushId", flushId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildLog).ToList();
    }

    // Función que registra eventos de trazabilidad y seguridad en la bitácora inmutable del sistema (WORM).
    private static SaturationLog BuildLog(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        FlushId = (int)row["FlushId"],
        PreviousInventory = (decimal)row["PreviousInventory"],
        NewInventory = (decimal)row["NewInventory"],
        ExcessEnergy = (decimal)row["ExcessEnergy"],
        EventDate = (DateTime)row["EventDate"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
