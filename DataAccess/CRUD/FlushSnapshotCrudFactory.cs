using Microsoft.Data.SqlClient;
using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para FlushSnapshot → tblFlushSnapshot (§12.12). WORM.
public class FlushSnapshotCrudFactory : CrudFactory
{
    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public override void Create(BaseDTO baseDTO)
    {
        var s = (FlushSnapshot)baseDTO;
        var op = new Operation { ProcedureName = "CRE_FLUSH_SNAP_PR" };
        op.AddIntParameter("@FlushId", s.FlushId);
        op.AddIntParameter("@TurbineId", s.TurbineId);
        op.AddIntParameter("@LocalBatteryId", s.LocalBatteryId);
        op.AddDecimalParameter("@CapturedEnergy", s.CapturedEnergy);
        op.AddDateTimeParameter("@EventDate", s.EventDate);
        op.AddDateTimeParameter("@Created", s.Created);
        sqlDao.ExecuteProcedure(op);
    }

    // Invoca el SP de modificación para actualizar los campos operacionales del registro en la base de datos.
    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Update is not supported for FlushSnapshot (WORM).");

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for FlushSnapshot (WORM).");

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_FLUSH_SNAP_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildSnapshot(results[0]) : default!;
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override List<T> RetrieveAll<T>() =>
        throw new NotSupportedException("RetrieveAll is not supported for FlushSnapshot.");

    // --- Overload transaccional (§37.25) ---

    public void Create(BaseDTO baseDTO, SqlConnection conn, SqlTransaction tx)
    {
        var s = (FlushSnapshot)baseDTO;
        var op = new Operation { ProcedureName = "CRE_FLUSH_SNAP_PR" };
        op.AddIntParameter("@FlushId", s.FlushId);
        op.AddIntParameter("@TurbineId", s.TurbineId);
        op.AddIntParameter("@LocalBatteryId", s.LocalBatteryId);
        op.AddDecimalParameter("@CapturedEnergy", s.CapturedEnergy);
        op.AddDateTimeParameter("@EventDate", s.EventDate);
        op.AddDateTimeParameter("@Created", s.Created);
        sqlDao.ExecuteProcedureInTransaction(op, conn, tx);
    }

    // --- Custom methods ---

    public List<FlushSnapshot> RetrieveByFlush(int flushId)
    {
        var op = new Operation { ProcedureName = "RET_BY_FLUSH_SNAP_PR" };
        op.AddIntParameter("@FlushId", flushId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildSnapshot).ToList();
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public List<FlushSnapshot> RetrieveByTurbine(int turbineId)
    {
        var op = new Operation { ProcedureName = "RET_BY_TURBINE_SNAP_PR" };
        op.AddIntParameter("@TurbineId", turbineId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildSnapshot).ToList();
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private static FlushSnapshot BuildSnapshot(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        FlushId = (int)row["FlushId"],
        TurbineId = (int)row["TurbineId"],
        LocalBatteryId = (int)row["LocalBatteryId"],
        CapturedEnergy = (decimal)row["CapturedEnergy"],
        EventDate = (DateTime)row["EventDate"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
