using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para Maintenance → tblMaintenances (§12.6).
public class MaintenanceCrudFactory : CrudFactory
{
    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public override void Create(BaseDTO baseDTO)
    {
        var m = (Maintenance)baseDTO;
        var op = new Operation { ProcedureName = "CRE_MAINT_PR" };
        op.AddIntParameter("@TurbineId", m.TurbineId);
        op.AddStringParameter("@MaintenanceType", m.MaintenanceType);
        op.AddDateTimeParameter("@EstimatedStartDate", m.EstimatedStartDate);
        op.AddDateTimeParameter("@EstimatedEndDate", m.EstimatedEndDate);
        op.AddStringParameter("@Status", m.Status);
        op.AddDateTimeParameter("@Created", m.Created);
        sqlDao.ExecuteProcedure(op);
    }

    // Función encargada de modificar y actualizar los campos operacionales de registros existentes.
    public override void Update(BaseDTO baseDTO)
    {
        var m = (Maintenance)baseDTO;
        var op = new Operation { ProcedureName = "UPD_MAINT_PR" };
        op.AddIntParameter("@Id", m.Id);
        op.AddNullableDateTimeParameter("@ActualStartDate", m.ActualStartDate);
        op.AddNullableDateTimeParameter("@ActualEndDate", m.ActualEndDate);
        op.AddStringParameter("@Result", m.Result);
        op.AddStringParameter("@Status", m.Status);
        op.AddDateTimeParameter("@Updated", m.Updated ?? DateTime.Now);
        sqlDao.ExecuteProcedure(op);
    }

    // Función encargada de realizar el borrado lógico o desactivación de registros según las políticas del sistema.
    public override void Delete(BaseDTO baseDTO)
    {
        var op = new Operation { ProcedureName = "DEL_MAINT_PR" };
        op.AddIntParameter("@Id", baseDTO.Id);
        sqlDao.ExecuteProcedure(op);
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_MAINT_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildMaintenance(results[0]) : default!;
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override List<T> RetrieveAll<T>()
    {
        var op = new Operation { ProcedureName = "RET_ALL_MAINT_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(r => (T)(object)BuildMaintenance(r)).ToList();
    }

    // --- Custom methods ---

    public List<Maintenance> RetrieveByTurbine(int turbineId)
    {
        var op = new Operation { ProcedureName = "RET_BY_TURBINE_MAINT_PR" };
        op.AddIntParameter("@TurbineId", turbineId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildMaintenance).ToList();
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public Maintenance? RetrieveActivePreventive()
    {
        var op = new Operation { ProcedureName = "RET_ACTIVE_PREV_MAINT_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildMaintenance(results[0]) : null;
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public void Complete(int maintenanceId, DateTime actualEndDate, string result, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_COMPLETE_MAINT_PR" };
        op.AddIntParameter("@Id", maintenanceId);
        op.AddDateTimeParameter("@ActualEndDate", actualEndDate);
        op.AddStringParameter("@Result", result);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private static Maintenance BuildMaintenance(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        TurbineId = (int)row["TurbineId"],
        MaintenanceType = (string)row["MaintenanceType"],
        EstimatedStartDate = (DateTime)row["EstimatedStartDate"],
        EstimatedEndDate = (DateTime)row["EstimatedEndDate"],
        ActualStartDate = row["ActualStartDate"] as DateTime?,
        ActualEndDate = row["ActualEndDate"] as DateTime?,
        Result = row["Result"] as string,
        Status = (string)row["Status"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
