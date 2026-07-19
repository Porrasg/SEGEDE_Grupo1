using Microsoft.Data.SqlClient;
using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para CentralBank → tblCentralBank (§12.14). Singleton (Id=1).
public class CentralBankCrudFactory : CrudFactory
{
    // Ejecuta el Stored Procedure de creación parametrizado en la base de datos SQL sin utilizar ORM.
    public override void Create(BaseDTO baseDTO) =>
        throw new NotSupportedException("Create is not supported for CentralBank (Singleton).");

    // Invoca el SP de modificación para actualizar los campos operacionales del registro en la base de datos.
    public override void Update(BaseDTO baseDTO) =>
        throw new NotSupportedException("Use specific update methods for CentralBank.");

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Delete is not supported for CentralBank (Singleton).");

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override T RetrieveById<T>(int id) =>
        throw new NotSupportedException("Use RetrieveSingleton for CentralBank.");

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override List<T> RetrieveAll<T>() =>
        throw new NotSupportedException("Use RetrieveSingleton for CentralBank.");

    // --- Custom methods ---

    public CentralBank? RetrieveSingleton()
    {
        var op = new Operation { ProcedureName = "RET_SINGLETON_CENT_BANK_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildCentralBank(results[0]) : null;
    }

    // Inicialización única e idempotente de la fila singleton (Id=1). No usar Create(): está bloqueado a propósito
    // para impedir inserciones fuera de este flujo de arranque/seed (§12.14, Singleton).
    public void InitializeSingleton(decimal currentInventory, decimal automaticCapacity, DateTime created)
    {
        var op = new Operation { ProcedureName = "INIT_CENT_BANK_PR" };
        op.AddDecimalParameter("@CurrentInventory", currentInventory);
        op.AddDecimalParameter("@AutomaticCapacity", automaticCapacity);
        op.AddDateTimeParameter("@Created", created);
        sqlDao.ExecuteProcedure(op);
    }

    // Función encargada de modificar y actualizar los campos operacionales de registros existentes.
    public void UpdateInventory(decimal currentInventory, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_INVENTORY_CENT_BANK_PR" };
        op.AddDecimalParameter("@CurrentInventory", currentInventory);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    // Función encargada de modificar y actualizar los campos operacionales de registros existentes.
    public void UpdateAutomaticCapacity(decimal automaticCapacity, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_AUTO_CAP_CENT_BANK_PR" };
        op.AddDecimalParameter("@AutomaticCapacity", automaticCapacity);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    // Función encargada de modificar y actualizar los campos operacionales de registros existentes.
    public void UpdateManualCapacity(decimal? manualCapacity, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_MANUAL_CAP_CENT_BANK_PR" };
        op.AddNullableDecimalParameter("@ManualCapacity", manualCapacity);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    // --- Overloads transaccionales (§37.25, contratos de bloqueo de Flush y Distribución) ---

    public CentralBank? RetrieveSingleton(SqlConnection conn, SqlTransaction tx)
    {
        var op = new Operation { ProcedureName = "RET_SINGLETON_CENT_BANK_PR" };
        var results = sqlDao.ExecuteQueryInTransaction(op, conn, tx);
        return results.Count > 0 ? BuildCentralBank(results[0]) : null;
    }

    public void UpdateInventory(decimal currentInventory, DateTime updated, SqlConnection conn, SqlTransaction tx)
    {
        var op = new Operation { ProcedureName = "UPD_INVENTORY_CENT_BANK_PR" };
        op.AddDecimalParameter("@CurrentInventory", currentInventory);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedureInTransaction(op, conn, tx);
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private static CentralBank BuildCentralBank(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        CurrentInventory = (decimal)row["CurrentInventory"],
        ManualCapacity = row["ManualCapacity"] as decimal?,
        AutomaticCapacity = (decimal)row["AutomaticCapacity"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
