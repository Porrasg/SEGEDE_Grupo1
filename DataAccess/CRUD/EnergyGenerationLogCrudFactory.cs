using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para EnergyGenerationLog → tblEnergyGenerationLog (§12.8). WORM.
public class EnergyGenerationLogCrudFactory : CrudFactory
{
    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public override void Create(BaseDTO baseDTO)
    {
        var g = (EnergyGenerationLog)baseDTO;
        var op = new Operation { ProcedureName = "CRE_EG_LOG_PR" };
        op.AddIntParameter("@TurbineId", g.TurbineId);
        op.AddDecimalParameter("@ActiveTimeSeconds", g.ActiveTimeSeconds);
        op.AddDecimalParameter("@GeneratedEnergy", g.GeneratedEnergy);
        op.AddDateTimeParameter("@EventDate", g.EventDate);
        op.AddDateTimeParameter("@Created", g.Created);
        sqlDao.ExecuteProcedure(op);
    }

    // Invoca el SP de modificación para actualizar los campos operacionales del registro en la base de datos.
    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Update is not supported for EnergyGenerationLog (WORM).");

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for EnergyGenerationLog (WORM).");

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_EG_LOG_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildLog(results[0]) : default!;
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override List<T> RetrieveAll<T>() =>
        throw new NotSupportedException("RetrieveAll is not supported for EnergyGenerationLog.");

    // --- Custom methods ---

    public List<EnergyGenerationLog> RetrieveByTurbine(int turbineId)
    {
        var op = new Operation { ProcedureName = "RET_BY_TURBINE_EG_LOG_PR" };
        op.AddIntParameter("@TurbineId", turbineId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildLog).ToList();
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public List<EnergyGenerationLog> RetrievePagedByTurbine(int turbineId, int pageNumber, int pageSize)
    {
        var op = new Operation { ProcedureName = "RET_PAGED_BY_TURBINE_EG_LOG_PR" };
        op.AddIntParameter("@TurbineId", turbineId);
        op.AddIntParameter("@PageNumber", pageNumber);
        op.AddIntParameter("@PageSize", pageSize);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildLog).ToList();
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public (decimal TotalActiveSeconds, decimal TotalGeneratedEnergy) RetrieveSumByTurbine(int turbineId)
    {
        var op = new Operation { ProcedureName = "RET_SUM_BY_TURBINE_EG_LOG_PR" };
        op.AddIntParameter("@TurbineId", turbineId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        if (results.Count > 0)
        {
            var row = results[0];
            return (
                row.ContainsKey("TotalActiveSeconds") && row["TotalActiveSeconds"] != DBNull.Value ? Convert.ToDecimal(row["TotalActiveSeconds"]) : 0m,
                row.ContainsKey("TotalGeneratedEnergy") && row["TotalGeneratedEnergy"] != DBNull.Value ? Convert.ToDecimal(row["TotalGeneratedEnergy"]) : 0m
            );
        }
        return (0m, 0m);
    }

    // Función que registra eventos de trazabilidad y seguridad en la bitácora inmutable del sistema (WORM).
    private static EnergyGenerationLog BuildLog(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        TurbineId = (int)row["TurbineId"],
        ActiveTimeSeconds = (decimal)row["ActiveTimeSeconds"],
        GeneratedEnergy = (decimal)row["GeneratedEnergy"],
        EventDate = (DateTime)row["EventDate"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
