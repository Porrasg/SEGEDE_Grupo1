using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para NotificationQueue → tblNotificationQueue (§12.22).
public class NotificationQueueCrudFactory : CrudFactory
{
    public override void Create(BaseDTO baseDTO)
    {
        var n = (NotificationQueue)baseDTO;
        var op = new Operation { ProcedureName = "CRE_NOTIF_PR" };
        op.AddIntParameter("@UserId", n.UserId);
        op.AddStringParameter("@RecipientEmail", n.RecipientEmail);
        op.AddStringParameter("@NotificationType", n.NotificationType);
        op.AddStringParameter("@Subject", n.Subject);
        op.AddStringParameter("@Body", n.Body);
        op.AddBoolParameter("@IsCritical", n.IsCritical);
        op.AddStringParameter("@Status", n.Status);
        op.AddIntParameter("@Attempts", n.Attempts);
        op.AddNullableDateTimeParameter("@NextAttempt", n.NextAttempt);
        op.AddNullableDateTimeParameter("@SentDate", n.SentDate);
        op.AddDateTimeParameter("@Created", n.Created);
        sqlDao.ExecuteProcedure(op);
    }

    public override void Update(BaseDTO baseDTO)
    {
        var n = (NotificationQueue)baseDTO;
        var op = new Operation { ProcedureName = "UPD_NOTIF_PR" };
        op.AddIntParameter("@Id", n.Id);
        op.AddStringParameter("@Status", n.Status);
        op.AddIntParameter("@Attempts", n.Attempts);
        op.AddNullableDateTimeParameter("@NextAttempt", n.NextAttempt);
        op.AddNullableDateTimeParameter("@SentDate", n.SentDate);
        op.AddDateTimeParameter("@Updated", n.Updated);
        sqlDao.ExecuteProcedure(op);
    }

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for NotificationQueue.");

    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_NOTIF_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildNotif(results[0]) : default!;
    }

    public override List<T> RetrieveAll<T>() =>
        throw new NotSupportedException("Use RetrieveByUser or RetrievePending for NotificationQueue.");

    // --- Custom methods ---

    public List<NotificationQueue> RetrievePending()
    {
        var op = new Operation { ProcedureName = "RET_PENDING_NOTIF_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildNotif).ToList();
    }

    public List<NotificationQueue> RetrieveByUser(int userId, int pageNumber, int pageSize)
    {
        var op = new Operation { ProcedureName = "RET_BY_USER_NOTIF_PR" };
        op.AddIntParameter("@UserId", userId);
        op.AddIntParameter("@PageNumber", pageNumber);
        op.AddIntParameter("@PageSize", pageSize);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildNotif).ToList();
    }

    private static NotificationQueue BuildNotif(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        UserId = (int)row["UserId"],
        RecipientEmail = (string)row["RecipientEmail"],
        NotificationType = (string)row["NotificationType"],
        Subject = (string)row["Subject"],
        Body = (string)row["Body"],
        IsCritical = (bool)row["IsCritical"],
        Status = (string)row["Status"],
        Attempts = (int)row["Attempts"],
        NextAttempt = row["NextAttempt"] as DateTime?,
        SentDate = row["SentDate"] as DateTime?,
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
