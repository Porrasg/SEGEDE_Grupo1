using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para LocalBattery → tblLocalBattery (§12.5).
public class LocalBatteryCrudFactory : CrudFactory
{
    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public override void Create(BaseDTO baseDTO)
    {
        var b = (LocalBattery)baseDTO;
        var op = new Operation { ProcedureName = "CRE_LOCAL_BAT_PR" };
        op.AddIntParameter("@TurbineId", b.TurbineId);
        op.AddDecimalParameter("@StoredEnergy", b.StoredEnergy);
        op.AddDateTimeParameter("@Created", b.Created);
        sqlDao.ExecuteProcedure(op);
    }

    // Invoca el SP de modificación para actualizar los campos operacionales del registro en la base de datos.
    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Use UpdateEnergy method instead.");

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for LocalBattery.");

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_LOCAL_BAT_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildBattery(results[0]) : default!;
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override List<T> RetrieveAll<T>()
    {
        var op = new Operation { ProcedureName = "RET_ALL_LOCAL_BAT_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(r => (T)(object)BuildBattery(r)).ToList();
    }

    // --- Custom methods ---

    public LocalBattery? RetrieveByTurbine(int turbineId)
    {
        var op = new Operation { ProcedureName = "RET_BY_TURBINE_LOCAL_BAT_PR" };
        op.AddIntParameter("@TurbineId", turbineId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildBattery(results[0]) : null;
    }

    // Función encargada de modificar y actualizar los campos operacionales de registros existentes.
    public void UpdateEnergy(int id, decimal storedEnergy, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_ENERGY_LOCAL_BAT_PR" };
        op.AddIntParameter("@Id", id);
        op.AddDecimalParameter("@StoredEnergy", storedEnergy);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public List<LocalBattery> RetrieveAllNonEmpty()
    {
        var op = new Operation { ProcedureName = "RET_ALL_NONEMPTY_LOCAL_BAT_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildBattery).ToList();
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private static LocalBattery BuildBattery(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        TurbineId = (int)row["TurbineId"],
        StoredEnergy = (decimal)row["StoredEnergy"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
