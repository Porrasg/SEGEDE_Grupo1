using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

/// <summary>
/// CrudFactory para ExportLog → tblExportLog (§12.24). WORM.
/// </summary>
public class ExportLogCrudFactory : CrudFactory
{
    public override void Create(BaseDTO baseDTO)
    {
        var e = (ExportLog)baseDTO;
        var op = new Operation { ProcedureName = "CRE_EXPORT_LOG_PR" };
        op.AddIntParameter("@UserId", e.UserId);
        op.AddStringParameter("@DocumentType", e.DocumentType);
        op.AddIntParameter("@DocumentId", e.DocumentId);
        op.AddStringParameter("@Format", e.Format);
        op.AddStringParameter("@CloneFilePath", e.CloneFilePath);
        op.AddDateTimeParameter("@EventDate", e.EventDate);
        op.AddDateTimeParameter("@Created", e.Created);
        sqlDao.ExecuteProcedure(op);
    }

    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Update is not supported for ExportLog (WORM).");

    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for ExportLog (WORM).");

    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_EXPORT_LOG_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildLog(results[0]) : default!;
    }

    public override List<T> RetrieveAll<T>()
    {
        var op = new Operation { ProcedureName = "RET_ALL_EXPORT_LOG_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(r => (T)(object)BuildLog(r)).ToList();
    }

    // --- Custom methods ---

    public List<ExportLog> RetrieveByUser(int userId, int pageNumber, int pageSize)
    {
        var op = new Operation { ProcedureName = "RET_BY_USER_EXPORT_LOG_PR" };
        op.AddIntParameter("@UserId", userId);
        op.AddIntParameter("@PageNumber", pageNumber);
        op.AddIntParameter("@PageSize", pageSize);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildLog).ToList();
    }

    public List<ExportLog> RetrieveAllPaged(int pageNumber, int pageSize)
    {
        var op = new Operation { ProcedureName = "RET_ALL_EXPORT_LOG_PR" };
        op.AddIntParameter("@PageNumber", pageNumber);
        op.AddIntParameter("@PageSize", pageSize);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildLog).ToList();
    }

    private static ExportLog BuildLog(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        UserId = (int)row["UserId"],
        DocumentType = (string)row["DocumentType"],
        DocumentId = (int)row["DocumentId"],
        Format = (string)row["Format"],
        CloneFilePath = (string)row["CloneFilePath"],
        EventDate = (DateTime)row["EventDate"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
