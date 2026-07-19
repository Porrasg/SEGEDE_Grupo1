using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para User → tblUsers (§12.1).
// 13 SPs: CRUD base + 8 custom.
public class UserCrudFactory : CrudFactory
{
    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
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

    // Función encargada de modificar y actualizar los campos operacionales de registros existentes.
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
        op.AddDateTimeParameter("@Updated", user.Updated ?? DateTime.Now);
        sqlDao.ExecuteProcedure(op);
    }

    // Función encargada de realizar el borrado lógico o desactivación de registros según las políticas del sistema.
    public override void Delete(BaseDTO baseDTO)
    {
        var op = new Operation { ProcedureName = "DEL_USER_PR" };
        op.AddIntParameter("@Id", baseDTO.Id);
        sqlDao.ExecuteProcedure(op);
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_USER_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildUser(results[0]) : default!;
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
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

    // Función encargada de modificar y actualizar los campos operacionales de registros existentes.
    public void UpdateStatus(int userId, string status, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_STATUS_USER_PR" };
        op.AddIntParameter("@Id", userId);
        op.AddStringParameter("@Status", status);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public void IncrementFailedAttempts(int userId)
    {
        var op = new Operation { ProcedureName = "UPD_ATTEMPTS_USER_PR" };
        op.AddIntParameter("@Id", userId);
        sqlDao.ExecuteProcedure(op);
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public void ResetFailedAttempts(int userId)
    {
        var op = new Operation { ProcedureName = "UPD_RESET_ATTEMPTS_USER_PR" };
        op.AddIntParameter("@Id", userId);
        sqlDao.ExecuteProcedure(op);
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public void BlockUser(int userId, DateTime blockedAt, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_BLOCK_USER_PR" };
        op.AddIntParameter("@Id", userId);
        op.AddDateTimeParameter("@BlockedAt", blockedAt);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    // Ejecuta operaciones criptográficas para el resguardo y verificación segura de credenciales e integridad.
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

    // Ejecuta operaciones criptográficas para el resguardo y verificación segura de credenciales e integridad.
    public void UpdatePassword(int userId, string passwordHash, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_PASSWORD_USER_PR" };
        op.AddIntParameter("@Id", userId);
        op.AddStringParameter("@PasswordHash", passwordHash);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public List<User> RetrieveExpiredBlocks(DateTime threshold)
    {
        var op = new Operation { ProcedureName = "RET_EXPIRED_BLOCKS_USER_PR" };
        op.AddDateTimeParameter("@Threshold", threshold);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildUser).ToList();
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
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
