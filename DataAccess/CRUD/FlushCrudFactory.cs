using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para Flush → tblFlush (§12.11).
public class FlushCrudFactory : CrudFactory
{
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

    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_FLUSH_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildFlush(results[0]) : default!;
    }

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

    public Flush? RetrieveActive()
    {
        var op = new Operation { ProcedureName = "RET_ACTIVE_FLUSH_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildFlush(results[0]) : null;
    }

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
