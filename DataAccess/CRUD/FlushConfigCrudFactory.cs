using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para FlushConfig → tblFlushConfig (§12.10). Singleton (Id=1).
public class FlushConfigCrudFactory : CrudFactory
{
    // Ejecuta el Stored Procedure de creación parametrizado en la base de datos SQL sin utilizar ORM.
    public override void Create(BaseDTO baseDTO) =>
        throw new NotSupportedException("Create is not supported for FlushConfig (Singleton).");

    // Función encargada de modificar y actualizar los campos operacionales de registros existentes.
    public override void Update(BaseDTO baseDTO)
    {
        var c = (FlushConfig)baseDTO;
        UpdateSingleton(c.ExecutionTime, c.IsAutomatic, c.Updated ?? DateTime.Now);
    }

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for FlushConfig (Singleton).");

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override T RetrieveById<T>(int id) =>
        throw new NotSupportedException("Use RetrieveSingleton for FlushConfig.");

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override List<T> RetrieveAll<T>() =>
        throw new NotSupportedException("Use RetrieveSingleton for FlushConfig.");

    // --- Custom methods ---

    public FlushConfig? RetrieveSingleton()
    {
        var op = new Operation { ProcedureName = "RET_SINGLETON_FLUSH_CFG_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildConfig(results[0]) : null;
    }

    // Función encargada de modificar y actualizar los campos operacionales de registros existentes.
    public void UpdateSingleton(TimeSpan executionTime, bool isAutomatic, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_SINGLETON_FLUSH_CFG_PR" };
        op.AddTimeParameter("@ExecutionTime", executionTime);
        op.AddBoolParameter("@IsAutomatic", isAutomatic);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private static FlushConfig BuildConfig(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        ExecutionTime = (TimeSpan)row["ExecutionTime"],
        IsAutomatic = (bool)row["IsAutomatic"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
