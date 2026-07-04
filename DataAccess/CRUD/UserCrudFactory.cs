using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

/// <summary>
/// CrudFactory para User → tblUsers (§12.1).
/// 13 SPs: CRUD base + 8 custom.
/// </summary>
public class UserCrudFactory : CrudFactory
{
    public override void Create(BaseDTO baseDTO)
    {
        var user = (User)baseDTO;
        var op = new Operation { ProcedureName = "CRE_USER_PR" };
        op.AddStringParameter("@Identification", user.Identification);
        op.AddStringParameter("@FirstName", user.FirstName);
        op.AddStringParameter("@LastName", user.LastName);
        op.AddDateTimeParameter("@BirthDate", user.BirthDate);
        op.AddStringParameter("@Phone", user.Phone);
        op.AddStringParameter("@Email", user.Email);
        op.AddStringParameter("@PhotoUrl", user.PhotoUrl);
        op.AddStringParameter("@PasswordHash", user.PasswordHash);
        op.AddStringParameter("@Role", user.Role);
        op.AddStringParameter("@Status", user.Status);
        op.AddDateTimeParameter("@Created", user.Created);
        sqlDao.ExecuteProcedure(op);
    }

    public override void Update(BaseDTO baseDTO)
    {
        var user = (User)baseDTO;
        var op = new Operation { ProcedureName = "UPD_USER_PR" };
        op.AddIntParameter("@Id", user.Id);
        op.AddStringParameter("@FirstName", user.FirstName);
        op.AddStringParameter("@LastName", user.LastName);
        op.AddStringParameter("@Phone", user.Phone);
        op.AddStringParameter("@Role", user.Role);
        op.AddStringParameter("@Status", user.Status);
        op.AddDateTimeParameter("@Updated", user.Updated);
        sqlDao.ExecuteProcedure(op);
    }

    public override void Delete(BaseDTO baseDTO)
    {
        var op = new Operation { ProcedureName = "DEL_USER_PR" };
        op.AddIntParameter("@Id", baseDTO.Id);
        sqlDao.ExecuteProcedure(op);
    }

    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_USER_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildUser(results[0]) : default!;
    }

    public override List<T> RetrieveAll<T>()
    {
        var op = new Operation { ProcedureName = "RET_ALL_USER_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(r => (T)(object)BuildUser(r)).ToList();
    }

    // --- Custom methods ---

    public User? RetrieveByEmail(string email)
    {
        var op = new Operation { ProcedureName = "RET_EMAIL_USER_PR" };
        op.AddStringParameter("@Email", email);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildUser(results[0]) : null;
    }

    public void UpdateStatus(int userId, string status, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_STATUS_USER_PR" };
        op.AddIntParameter("@Id", userId);
        op.AddStringParameter("@Status", status);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    public void IncrementFailedAttempts(int userId)
    {
        var op = new Operation { ProcedureName = "UPD_ATTEMPTS_USER_PR" };
        op.AddIntParameter("@Id", userId);
        sqlDao.ExecuteProcedure(op);
    }

    public void ResetFailedAttempts(int userId)
    {
        var op = new Operation { ProcedureName = "UPD_RESET_ATTEMPTS_USER_PR" };
        op.AddIntParameter("@Id", userId);
        sqlDao.ExecuteProcedure(op);
    }

    public void BlockUser(int userId, DateTime blockedAt, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_BLOCK_USER_PR" };
        op.AddIntParameter("@Id", userId);
        op.AddDateTimeParameter("@BlockedAt", blockedAt);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    public void UpdateProfile(int userId, string phone, string? photoUrl, string? passwordHash, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_PROFILE_USER_PR" };
        op.AddIntParameter("@Id", userId);
        op.AddStringParameter("@Phone", phone);
        op.AddStringParameter("@PhotoUrl", photoUrl);
        op.AddStringParameter("@PasswordHash", passwordHash);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    public void UpdatePassword(int userId, string passwordHash, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_PASSWORD_USER_PR" };
        op.AddIntParameter("@Id", userId);
        op.AddStringParameter("@PasswordHash", passwordHash);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    public List<User> RetrieveExpiredBlocks(DateTime threshold)
    {
        var op = new Operation { ProcedureName = "RET_EXPIRED_BLOCKS_USER_PR" };
        op.AddDateTimeParameter("@Threshold", threshold);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildUser).ToList();
    }

    private static User BuildUser(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        Identification = (string)row["Identification"],
        FirstName = (string)row["FirstName"],
        LastName = (string)row["LastName"],
        BirthDate = (DateTime)row["BirthDate"],
        Phone = (string)row["Phone"],
        Email = (string)row["Email"],
        PhotoUrl = row["PhotoUrl"] as string,
        PasswordHash = (string)row["PasswordHash"],
        Role = (string)row["Role"],
        Status = (string)row["Status"],
        FailedAttempts = (int)row["FailedAttempts"],
        BlockedAt = row["BlockedAt"] as DateTime?,
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
