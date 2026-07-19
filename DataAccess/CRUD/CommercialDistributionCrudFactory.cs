using Microsoft.Data.SqlClient;
using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para CommercialDistribution → tblCommercialDistribution (§12.17).
public class CommercialDistributionCrudFactory : CrudFactory
{
    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public override void Create(BaseDTO baseDTO)
    {
        var d = (CommercialDistribution)baseDTO;
        var op = new Operation { ProcedureName = "CRE_COMM_DIST_PR" };
        op.AddIntParameter("@Month", d.Month);
        op.AddIntParameter("@Year", d.Year);
        op.AddDateTimeParameter("@ExecutionDate", d.ExecutionDate);
        op.AddDecimalParameter("@AvailableInventory", d.AvailableInventory);
        op.AddDecimalParameter("@TotalDemand", d.TotalDemand);
        op.AddDecimalParameter("@DistributedEnergy", d.DistributedEnergy);
        op.AddDecimalParameter("@RoundingResidual", d.RoundingResidual);
        op.AddStringParameter("@Scenario", d.Scenario);
        op.AddDateTimeParameter("@Created", d.Created);
        sqlDao.ExecuteProcedure(op);
    }

    // Invoca el SP de modificación para actualizar los campos operacionales del registro en la base de datos.
    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Update is not supported for CommercialDistribution.");

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for CommercialDistribution.");

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_COMM_DIST_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildDistribution(results[0]) : default!;
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override List<T> RetrieveAll<T>()
    {
        var op = new Operation { ProcedureName = "RET_ALL_COMM_DIST_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(r => (T)(object)BuildDistribution(r)).ToList();
    }

    // --- Custom methods ---

    public CommercialDistribution? RetrieveByMonth(int month, int year)
    {
        var op = new Operation { ProcedureName = "RET_BY_MONTH_COMM_DIST_PR" };
        op.AddIntParameter("@Month", month);
        op.AddIntParameter("@Year", year);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildDistribution(results[0]) : null;
    }

    // --- Overloads transaccionales (§37.25, ciclo ACID de Distribución Comercial) ---

    public void Create(BaseDTO baseDTO, SqlConnection conn, SqlTransaction tx)
    {
        var d = (CommercialDistribution)baseDTO;
        var op = new Operation { ProcedureName = "CRE_COMM_DIST_PR" };
        op.AddIntParameter("@Month", d.Month);
        op.AddIntParameter("@Year", d.Year);
        op.AddDateTimeParameter("@ExecutionDate", d.ExecutionDate);
        op.AddDecimalParameter("@AvailableInventory", d.AvailableInventory);
        op.AddDecimalParameter("@TotalDemand", d.TotalDemand);
        op.AddDecimalParameter("@DistributedEnergy", d.DistributedEnergy);
        op.AddDecimalParameter("@RoundingResidual", d.RoundingResidual);
        op.AddStringParameter("@Scenario", d.Scenario);
        op.AddDateTimeParameter("@Created", d.Created);
        sqlDao.ExecuteProcedureInTransaction(op, conn, tx);
    }

    public CommercialDistribution? RetrieveByMonth(int month, int year, SqlConnection conn, SqlTransaction tx)
    {
        var op = new Operation { ProcedureName = "RET_BY_MONTH_COMM_DIST_PR" };
        op.AddIntParameter("@Month", month);
        op.AddIntParameter("@Year", year);
        var results = sqlDao.ExecuteQueryInTransaction(op, conn, tx);
        return results.Count > 0 ? BuildDistribution(results[0]) : null;
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private static CommercialDistribution BuildDistribution(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        Month = (int)row["Month"],
        Year = (int)row["Year"],
        ExecutionDate = (DateTime)row["ExecutionDate"],
        AvailableInventory = (decimal)row["AvailableInventory"],
        TotalDemand = (decimal)row["TotalDemand"],
        DistributedEnergy = (decimal)row["DistributedEnergy"],
        RoundingResidual = (decimal)row["RoundingResidual"],
        Scenario = (string)row["Scenario"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
