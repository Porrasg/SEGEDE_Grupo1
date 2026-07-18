using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para Turbine → tblTurbines (§12.3).
public class TurbineCrudFactory : CrudFactory
{
    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public override void Create(BaseDTO baseDTO)
    {
        var t = (Turbine)baseDTO;
        var op = new Operation { ProcedureName = "CRE_TURBINE_PR" };
        op.AddStringParameter("@UniqueCode", t.UniqueCode);
        op.AddStringParameter("@Name", t.Name);
        op.AddStringParameter("@Location", t.Location);
        op.AddStringParameter("@Brand", t.Brand);
        op.AddStringParameter("@Model", t.Model);
        op.AddIntParameter("@Year", t.Year);
        op.AddDecimalParameter("@WeeklyNominalCapacity", t.WeeklyNominalCapacity);
        op.AddStringParameter("@Status", t.Status);
        op.AddDateTimeParameter("@LastStateChange", t.LastStateChange);
        op.AddDateTimeParameter("@Created", t.Created);
        sqlDao.ExecuteProcedure(op);
    }

    // Función encargada de modificar y actualizar los campos operacionales de registros existentes.
    public override void Update(BaseDTO baseDTO)
    {
        var t = (Turbine)baseDTO;
        var op = new Operation { ProcedureName = "UPD_TURBINE_PR" };
        op.AddIntParameter("@Id", t.Id);
        op.AddStringParameter("@Name", t.Name);
        op.AddStringParameter("@Location", t.Location);
        op.AddStringParameter("@Brand", t.Brand);
        op.AddStringParameter("@Model", t.Model);
        op.AddDecimalParameter("@WeeklyNominalCapacity", t.WeeklyNominalCapacity);
        op.AddDateTimeParameter("@Updated", t.Updated ?? DateTime.Now);
        sqlDao.ExecuteProcedure(op);
    }

    // Función encargada de realizar el borrado lógico o desactivación de registros según las políticas del sistema.
    public override void Delete(BaseDTO baseDTO)
    {
        var op = new Operation { ProcedureName = "DEL_TURBINE_PR" };
        op.AddIntParameter("@Id", baseDTO.Id);
        sqlDao.ExecuteProcedure(op);
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_TURBINE_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildTurbine(results[0]) : default!;
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override List<T> RetrieveAll<T>()
    {
        var op = new Operation { ProcedureName = "RET_ALL_TURBINE_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(r => (T)(object)BuildTurbine(r)).ToList();
    }

    // --- Custom methods ---

    public List<Turbine> RetrieveAllActive()
    {
        var op = new Operation { ProcedureName = "RET_ALL_ACTIVE_TURBINE_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildTurbine).ToList();
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public Turbine? RetrieveByCode(string uniqueCode)
    {
        var op = new Operation { ProcedureName = "RET_BY_CODE_TURBINE_PR" };
        op.AddStringParameter("@UniqueCode", uniqueCode);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildTurbine(results[0]) : null;
    }

    // Función encargada de modificar y actualizar los campos operacionales de registros existentes.
    public void UpdateStatus(int turbineId, string status, DateTime lastStateChange, DateTime? lastMaintenance, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_STATUS_TURBINE_PR" };
        op.AddIntParameter("@Id", turbineId);
        op.AddStringParameter("@Status", status);
        op.AddDateTimeParameter("@LastStateChange", lastStateChange);
        op.AddNullableDateTimeParameter("@LastMaintenance", lastMaintenance);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public List<Turbine> RetrieveOverdue(DateTime threshold)
    {
        var op = new Operation { ProcedureName = "RET_OVERDUE_TURBINE_PR" };
        op.AddDateTimeParameter("@Threshold", threshold);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildTurbine).ToList();
    }

    // Función encargada de modificar y actualizar los campos operacionales de registros existentes.
    public void UpdateMaintenanceDate(int turbineId, DateTime lastMaintenance, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_MAINT_DATE_TURBINE_PR" };
        op.AddIntParameter("@Id", turbineId);
        op.AddDateTimeParameter("@LastMaintenance", lastMaintenance);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private static Turbine BuildTurbine(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        UniqueCode = (string)row["UniqueCode"],
        Name = (string)row["Name"],
        Location = (string)row["Location"],
        Brand = (string)row["Brand"],
        Model = (string)row["Model"],
        Year = (int)row["Year"],
        WeeklyNominalCapacity = (decimal)row["WeeklyNominalCapacity"],
        Status = (string)row["Status"],
        LastMaintenance = row["LastMaintenance"] as DateTime?,
        LastStateChange = (DateTime)row["LastStateChange"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
