using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para EnergyLossLog → tblEnergyLossLog (§12.9). WORM.
public class EnergyLossLogCrudFactory : CrudFactory
{
    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public override void Create(BaseDTO baseDTO)
    {
        var l = (EnergyLossLog)baseDTO;
        var op = new Operation { ProcedureName = "CRE_EL_LOG_PR" };
        op.AddIntParameter("@TurbineId", l.TurbineId);
        op.AddDecimalParameter("@InactiveTimeSeconds", l.InactiveTimeSeconds);
        op.AddDecimalParameter("@LostEnergy", l.LostEnergy);
        op.AddStringParameter("@Cause", l.Cause);
        op.AddDateTimeParameter("@EventDate", l.EventDate);
        op.AddDateTimeParameter("@Created", l.Created);
        sqlDao.ExecuteProcedure(op);
    }

    // Invoca el SP de modificación para actualizar los campos operacionales del registro en la base de datos.
    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Update is not supported for EnergyLossLog (WORM).");

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for EnergyLossLog (WORM).");

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_EL_LOG_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildLog(results[0]) : default!;
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override List<T> RetrieveAll<T>() =>
        throw new NotSupportedException("RetrieveAll is not supported for EnergyLossLog.");

    // --- Custom methods ---

    public List<EnergyLossLog> RetrieveByTurbine(int turbineId)
    {
        var op = new Operation { ProcedureName = "RET_BY_TURBINE_EL_LOG_PR" };
        op.AddIntParameter("@TurbineId", turbineId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildLog).ToList();
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public List<EnergyLossLog> RetrieveByCause(int turbineId, string cause)
    {
        var op = new Operation { ProcedureName = "RET_BY_CAUSE_EL_LOG_PR" };
        op.AddIntParameter("@TurbineId", turbineId);
        op.AddStringParameter("@Cause", cause);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildLog).ToList();
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public decimal RetrieveSumByTurbine(int turbineId)
    {
        var op = new Operation { ProcedureName = "RET_SUM_BY_TURBINE_EL_LOG_PR" };
        op.AddIntParameter("@TurbineId", turbineId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        if (results.Count > 0 && results[0].Values.First() != DBNull.Value)
        {
            return Convert.ToDecimal(results[0].Values.First());
        }
        return 0m;
    }

    // Función que registra eventos de trazabilidad y seguridad en la bitácora inmutable del sistema (WORM).
    private static EnergyLossLog BuildLog(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        TurbineId = (int)row["TurbineId"],
        InactiveTimeSeconds = (decimal)row["InactiveTimeSeconds"],
        LostEnergy = (decimal)row["LostEnergy"],
        Cause = (string)row["Cause"],
        EventDate = (DateTime)row["EventDate"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
