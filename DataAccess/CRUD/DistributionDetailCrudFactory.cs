using Microsoft.Data.SqlClient;
using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para DistributionDetail → tblDistributionDetail (§12.18).
public class DistributionDetailCrudFactory : CrudFactory
{
    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public override void Create(BaseDTO baseDTO)
    {
        var d = (DistributionDetail)baseDTO;
        var op = new Operation { ProcedureName = "CRE_DIST_DTL_PR" };
        op.AddIntParameter("@DistributionId", d.DistributionId);
        op.AddIntParameter("@BuyerId", d.BuyerId);
        op.AddIntParameter("@ForecastId", d.ForecastId);
        op.AddDecimalParameter("@RequestedMWh", d.RequestedMWh);
        op.AddDecimalParameter("@AssignedMWh", d.AssignedMWh);
        op.AddDecimalParameter("@UnsuppliedDemand", d.UnsuppliedDemand);
        op.AddDateTimeParameter("@Created", d.Created);
        sqlDao.ExecuteProcedure(op);
    }

    // Invoca el SP de modificación para actualizar los campos operacionales del registro en la base de datos.
    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Update is not supported for DistributionDetail.");

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for DistributionDetail.");

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_DIST_DTL_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildDetail(results[0]) : default!;
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override List<T> RetrieveAll<T>() =>
        throw new NotSupportedException("RetrieveAll is not supported for DistributionDetail.");

    // --- Overload transaccional (§37.25) ---

    public void Create(BaseDTO baseDTO, SqlConnection conn, SqlTransaction tx)
    {
        var d = (DistributionDetail)baseDTO;
        var op = new Operation { ProcedureName = "CRE_DIST_DTL_PR" };
        op.AddIntParameter("@DistributionId", d.DistributionId);
        op.AddIntParameter("@BuyerId", d.BuyerId);
        op.AddIntParameter("@ForecastId", d.ForecastId);
        op.AddDecimalParameter("@RequestedMWh", d.RequestedMWh);
        op.AddDecimalParameter("@AssignedMWh", d.AssignedMWh);
        op.AddDecimalParameter("@UnsuppliedDemand", d.UnsuppliedDemand);
        op.AddDateTimeParameter("@Created", d.Created);
        sqlDao.ExecuteProcedureInTransaction(op, conn, tx);
    }

    // --- Custom methods ---

    public List<DistributionDetail> RetrieveByDistribution(int distributionId)
    {
        var op = new Operation { ProcedureName = "RET_BY_DIST_DIST_DTL_PR" };
        op.AddIntParameter("@DistributionId", distributionId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildDetail).ToList();
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public List<DistributionDetail> RetrieveByBuyer(int buyerId)
    {
        var op = new Operation { ProcedureName = "RET_BY_BUYER_DIST_DTL_PR" };
        op.AddIntParameter("@BuyerId", buyerId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildDetail).ToList();
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private static DistributionDetail BuildDetail(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        DistributionId = (int)row["DistributionId"],
        BuyerId = (int)row["BuyerId"],
        ForecastId = (int)row["ForecastId"],
        RequestedMWh = (decimal)row["RequestedMWh"],
        AssignedMWh = (decimal)row["AssignedMWh"],
        UnsuppliedDemand = (decimal)row["UnsuppliedDemand"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
