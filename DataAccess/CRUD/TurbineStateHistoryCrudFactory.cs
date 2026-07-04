using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para TurbineStateHistory → tblTurbineStateHistory (§12.4). WORM log.
public class TurbineStateHistoryCrudFactory : CrudFactory
{
    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public override void Create(BaseDTO baseDTO)
    {
        var h = (TurbineStateHistory)baseDTO;
        var op = new Operation { ProcedureName = "CRE_TRB_STATE_PR" };
        op.AddIntParameter("@TurbineId", h.TurbineId);
        op.AddStringParameter("@PreviousState", h.PreviousState);
        op.AddStringParameter("@NewState", h.NewState);
        op.AddDateTimeParameter("@ChangeDate", h.ChangeDate);
        op.AddStringParameter("@Reason", h.Reason);
        op.AddIntParameter("@UserId", h.UserId);
        op.AddDateTimeParameter("@Created", h.Created);
        sqlDao.ExecuteProcedure(op);
    }

    // Invoca el SP de modificación para actualizar los campos operacionales del registro en la base de datos.
    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Update is not supported for TurbineStateHistory (WORM).");

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for TurbineStateHistory (WORM).");

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_TRB_STATE_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildHistory(results[0]) : default!;
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override List<T> RetrieveAll<T>()
    {
        var op = new Operation { ProcedureName = "RET_ALL_TRB_STATE_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(r => (T)(object)BuildHistory(r)).ToList();
    }

    // --- Custom methods ---

    public List<TurbineStateHistory> RetrieveByTurbine(int turbineId)
    {
        var op = new Operation { ProcedureName = "RET_BY_TURBINE_TRB_STATE_PR" };
        op.AddIntParameter("@TurbineId", turbineId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildHistory).ToList();
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private static TurbineStateHistory BuildHistory(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        TurbineId = (int)row["TurbineId"],
        PreviousState = (string)row["PreviousState"],
        NewState = (string)row["NewState"],
        ChangeDate = (DateTime)row["ChangeDate"],
        Reason = (string)row["Reason"],
        UserId = (int)row["UserId"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
