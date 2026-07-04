using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para OtpAttempt → tblOtpAttempts (§12.2).
public class OtpAttemptCrudFactory : CrudFactory
{
    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public override void Create(BaseDTO baseDTO)
    {
        var otp = (OtpAttempt)baseDTO;
        var op = new Operation { ProcedureName = "CRE_OTP_ATTEMPT_PR" };
        op.AddIntParameter("@UserId", otp.UserId);
        op.AddStringParameter("@UsageType", otp.UsageType);
        op.AddIntParameter("@ResendCount", otp.ResendCount);
        op.AddIntParameter("@FailedAttempts", otp.FailedAttempts);
        op.AddStringParameter("@Status", otp.Status);
        op.AddDateTimeParameter("@StartDate", otp.StartDate);
        op.AddDateTimeParameter("@WindowExpiration", otp.WindowExpiration);
        op.AddDateTimeParameter("@Created", otp.Created);
        sqlDao.ExecuteProcedure(op);
    }

    // Función encargada de modificar y actualizar los campos operacionales de registros existentes.
    public override void Update(BaseDTO baseDTO)
    {
        var otp = (OtpAttempt)baseDTO;
        var op = new Operation { ProcedureName = "UPD_OTP_ATTEMPT_PR" };
        op.AddIntParameter("@Id", otp.Id);
        op.AddIntParameter("@ResendCount", otp.ResendCount);
        op.AddIntParameter("@FailedAttempts", otp.FailedAttempts);
        op.AddStringParameter("@Status", otp.Status);
        op.AddDateTimeParameter("@Updated", otp.Updated);
        sqlDao.ExecuteProcedure(op);
    }

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for OtpAttempt.");

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_OTP_ATTEMPT_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildOtpAttempt(results[0]) : default!;
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override List<T> RetrieveAll<T>() =>
        throw new NotSupportedException("RetrieveAll is not supported for OtpAttempt.");

    // --- Custom methods ---

    public OtpAttempt? RetrieveActive(int userId, string usageType)
    {
        var op = new Operation { ProcedureName = "RET_ACTIVE_OTP_ATTEMPT_PR" };
        op.AddIntParameter("@UserId", userId);
        op.AddStringParameter("@UsageType", usageType);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildOtpAttempt(results[0]) : null;
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public void IncrementResendCount(int id)
    {
        var op = new Operation { ProcedureName = "UPD_RESEND_OTP_ATTEMPT_PR" };
        op.AddIntParameter("@Id", id);
        sqlDao.ExecuteProcedure(op);
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public void IncrementFailedAttempts(int id)
    {
        var op = new Operation { ProcedureName = "UPD_FAIL_OTP_ATTEMPT_PR" };
        op.AddIntParameter("@Id", id);
        sqlDao.ExecuteProcedure(op);
    }

    // Función encargada de modificar y actualizar los campos operacionales de registros existentes.
    public void UpdateStatus(int id, string status, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_STATUS_OTP_ATTEMPT_PR" };
        op.AddIntParameter("@Id", id);
        op.AddStringParameter("@Status", status);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private static OtpAttempt BuildOtpAttempt(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        UserId = (int)row["UserId"],
        UsageType = (string)row["UsageType"],
        ResendCount = (int)row["ResendCount"],
        FailedAttempts = (int)row["FailedAttempts"],
        Status = (string)row["Status"],
        StartDate = (DateTime)row["StartDate"],
        WindowExpiration = (DateTime)row["WindowExpiration"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
