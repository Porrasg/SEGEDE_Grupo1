using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para AuditLog → tblAuditLog (§12.23). WORM.
public class AuditLogCrudFactory : CrudFactory
{
    public override void Create(BaseDTO baseDTO)
    {
        var a = (AuditLog)baseDTO;
        var op = new Operation { ProcedureName = "CRE_AUDIT_LOG_PR" };
        op.AddNullableIntParameter("@UserId", a.UserId);
        op.AddStringParameter("@UserName", a.UserName);
        op.AddStringParameter("@Module", a.Module);
        op.AddStringParameter("@Action", a.Action);
        op.AddStringParameter("@AffectedEntity", a.AffectedEntity);
        op.AddIntParameter("@EntityId", a.EntityId);
        op.AddStringParameter("@PreviousValue", a.PreviousValue);
        op.AddStringParameter("@NewValue", a.NewValue);
        op.AddDateTimeParameter("@EventDate", a.EventDate);
        op.AddBoolParameter("@IsColdArchive", a.IsColdArchive);
        op.AddDateTimeParameter("@Created", a.Created);
        sqlDao.ExecuteProcedure(op);
    }

    // Invoca el SP de modificación para actualizar los campos operacionales del registro en la base de datos.
    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Update is not supported for AuditLog (WORM).");

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for AuditLog (WORM).");

    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_AUDIT_LOG_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildLog(results[0]) : default!;
    }

    public override List<T> RetrieveAll<T>() =>
        throw new NotSupportedException("Use paged retrieval methods for AuditLog.");

    // --- Custom methods ---

    public List<AuditLog> RetrieveByModule(string module, int pageNumber, int pageSize)
    {
        var op = new Operation { ProcedureName = "RET_BY_MODULE_AUDIT_LOG_PR" };
        op.AddStringParameter("@Module", module);
        op.AddIntParameter("@PageNumber", pageNumber);
        op.AddIntParameter("@PageSize", pageSize);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildLog).ToList();
    }

    public List<AuditLog> RetrieveByUser(int userId, int pageNumber, int pageSize)
    {
        var op = new Operation { ProcedureName = "RET_BY_USER_AUDIT_LOG_PR" };
        op.AddIntParameter("@UserId", userId);
        op.AddIntParameter("@PageNumber", pageNumber);
        op.AddIntParameter("@PageSize", pageSize);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildLog).ToList();
    }

    public List<AuditLog> RetrieveByDateRange(DateTime from, DateTime to, int pageNumber, int pageSize)
    {
        var op = new Operation { ProcedureName = "RET_BY_DATE_AUDIT_LOG_PR" };
        op.AddDateTimeParameter("@From", from);
        op.AddDateTimeParameter("@To", to);
        op.AddIntParameter("@PageNumber", pageNumber);
        op.AddIntParameter("@PageSize", pageSize);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildLog).ToList();
    }

    public void MarkColdArchive(DateTime threshold)
    {
        var op = new Operation { ProcedureName = "MARK_COLD_AUDIT_LOG_PR" };
        op.AddDateTimeParameter("@Threshold", threshold);
        sqlDao.ExecuteProcedure(op);
    }

    private static AuditLog BuildLog(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        UserId = row["UserId"] as int?,
        UserName = (string)row["UserName"],
        Module = (string)row["Module"],
        Action = (string)row["Action"],
        AffectedEntity = (string)row["AffectedEntity"],
        EntityId = (int)row["EntityId"],
        PreviousValue = row["PreviousValue"] as string,
        NewValue = row["NewValue"] as string,
        EventDate = (DateTime)row["EventDate"],
        IsColdArchive = (bool)row["IsColdArchive"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
