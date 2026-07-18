using Microsoft.Data.SqlClient;
using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para Flush → tblFlush (§12.11).
public class FlushCrudFactory : CrudFactory
{
    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public override void Create(BaseDTO baseDTO)
    {
        var f = (Flush)baseDTO;
        var op = new Operation { ProcedureName = "CRE_FLUSH_PR" };
        op.AddStringParameter("@ExecutionType", f.ExecutionType);
        op.AddStringParameter("@Status", f.Status);
        op.AddNullableIntParameter("@UserId", f.UserId);
        op.AddDecimalParameter("@TotalTransferredEnergy", f.TotalTransferredEnergy);
        op.AddDecimalParameter("@SaturationLoss", f.SaturationLoss);
        op.AddDateTimeParameter("@StartDate", f.StartDate);
        op.AddNullableDateTimeParameter("@EndDate", f.EndDate);
        op.AddDateTimeParameter("@Created", f.Created);
        sqlDao.ExecuteProcedure(op);
    }

    // Invoca el SP de modificación para actualizar los campos operacionales del registro en la base de datos.
    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Use UpdateStatus method instead.");

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for Flush.");

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_FLUSH_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildFlush(results[0]) : default!;
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override List<T> RetrieveAll<T>()
    {
        var op = new Operation { ProcedureName = "RET_ALL_FLUSH_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(r => (T)(object)BuildFlush(r)).ToList();
    }

    // --- Custom methods ---

    public List<Flush> RetrievePaged(int pageNumber, int pageSize)
    {
        var op = new Operation { ProcedureName = "RET_PAGED_FLUSH_PR" };
        op.AddIntParameter("@PageNumber", pageNumber);
        op.AddIntParameter("@PageSize", pageSize);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildFlush).ToList();
    }

    // Función encargada de modificar y actualizar los campos operacionales de registros existentes.
    public void UpdateStatus(int id, string status, DateTime? endDate, decimal totalTransferredEnergy, decimal saturationLoss, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_STATUS_FLUSH_PR" };
        op.AddIntParameter("@Id", id);
        op.AddStringParameter("@Status", status);
        op.AddNullableDateTimeParameter("@EndDate", endDate);
        op.AddDecimalParameter("@TotalTransferredEnergy", totalTransferredEnergy);
        op.AddDecimalParameter("@SaturationLoss", saturationLoss);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public Flush? RetrieveActive()
    {
        var op = new Operation { ProcedureName = "RET_ACTIVE_FLUSH_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildFlush(results[0]) : null;
    }

    // --- Overloads transaccionales (§37.25, contratos de bloqueo del ciclo de Flush ACID) ---

    public void Create(BaseDTO baseDTO, SqlConnection conn, SqlTransaction tx)
    {
        var f = (Flush)baseDTO;
        var op = new Operation { ProcedureName = "CRE_FLUSH_PR" };
        op.AddStringParameter("@ExecutionType", f.ExecutionType);
        op.AddStringParameter("@Status", f.Status);
        op.AddNullableIntParameter("@UserId", f.UserId);
        op.AddDecimalParameter("@TotalTransferredEnergy", f.TotalTransferredEnergy);
        op.AddDecimalParameter("@SaturationLoss", f.SaturationLoss);
        op.AddDateTimeParameter("@StartDate", f.StartDate);
        op.AddNullableDateTimeParameter("@EndDate", f.EndDate);
        op.AddDateTimeParameter("@Created", f.Created);
        sqlDao.ExecuteProcedureInTransaction(op, conn, tx);
    }

    public Flush? RetrieveActive(SqlConnection conn, SqlTransaction tx)
    {
        var op = new Operation { ProcedureName = "RET_ACTIVE_FLUSH_PR" };
        var results = sqlDao.ExecuteQueryInTransaction(op, conn, tx);
        return results.Count > 0 ? BuildFlush(results[0]) : null;
    }

    public void UpdateStatus(int id, string status, DateTime? endDate, decimal totalTransferredEnergy, decimal saturationLoss, DateTime updated, SqlConnection conn, SqlTransaction tx)
    {
        var op = new Operation { ProcedureName = "UPD_STATUS_FLUSH_PR" };
        op.AddIntParameter("@Id", id);
        op.AddStringParameter("@Status", status);
        op.AddNullableDateTimeParameter("@EndDate", endDate);
        op.AddDecimalParameter("@TotalTransferredEnergy", totalTransferredEnergy);
        op.AddDecimalParameter("@SaturationLoss", saturationLoss);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedureInTransaction(op, conn, tx);
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private static Flush BuildFlush(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        ExecutionType = (string)row["ExecutionType"],
        Status = (string)row["Status"],
        UserId = row["UserId"] as int?,
        TotalTransferredEnergy = (decimal)row["TotalTransferredEnergy"],
        SaturationLoss = (decimal)row["SaturationLoss"],
        StartDate = (DateTime)row["StartDate"],
        EndDate = row["EndDate"] as DateTime?,
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
