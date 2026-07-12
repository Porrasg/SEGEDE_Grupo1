using Microsoft.Data.SqlClient;
using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// CrudFactory para Forecast → tblForecast (§12.16).
public class ForecastCrudFactory : CrudFactory
{
    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public override void Create(BaseDTO baseDTO)
    {
        var f = (Forecast)baseDTO;
        var op = new Operation { ProcedureName = "CRE_FORECAST_PR" };
        op.AddIntParameter("@BuyerId", f.BuyerId);
        op.AddIntParameter("@Month", f.Month);
        op.AddIntParameter("@Year", f.Year);
        op.AddDecimalParameter("@AmountMWh", f.AmountMWh);
        op.AddStringParameter("@Status", f.Status);
        op.AddDateTimeParameter("@Created", f.Created);
        sqlDao.ExecuteProcedure(op);
    }

    // Función encargada de modificar y actualizar los campos operacionales de registros existentes.
    public override void Update(BaseDTO baseDTO)
    {
        var f = (Forecast)baseDTO;
        var op = new Operation { ProcedureName = "UPD_FORECAST_PR" };
        op.AddIntParameter("@Id", f.Id);
        op.AddDecimalParameter("@AmountMWh", f.AmountMWh);
        op.AddStringParameter("@Status", f.Status);
        op.AddDateTimeParameter("@Updated", f.Updated ?? DateTime.Now);
        sqlDao.ExecuteProcedure(op);
    }

    // Ejecuta el borrado lógico o desactivación del registro en la tabla relacional correspondiente.
    public override void Delete(BaseDTO baseDTO) =>
        throw new NotSupportedException("Use UpdateStatus method to cancel Forecast.");

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override T RetrieveById<T>(int id)
    {
        var op = new Operation { ProcedureName = "RET_ID_FORECAST_PR" };
        op.AddIntParameter("@Id", id);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? (T)(object)BuildForecast(results[0]) : default!;
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public override List<T> RetrieveAll<T>()
    {
        var op = new Operation { ProcedureName = "RET_ALL_FORECAST_PR" };
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(r => (T)(object)BuildForecast(r)).ToList();
    }

    // --- Custom methods ---

    public List<Forecast> RetrieveByBuyer(int buyerId)
    {
        var op = new Operation { ProcedureName = "RET_BY_BUYER_FORECAST_PR" };
        op.AddIntParameter("@BuyerId", buyerId);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildForecast).ToList();
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public List<Forecast> RetrieveByMonth(int month, int year)
    {
        var op = new Operation { ProcedureName = "RET_BY_MONTH_FORECAST_PR" };
        op.AddIntParameter("@Month", month);
        op.AddIntParameter("@Year", year);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Select(BuildForecast).ToList();
    }

    // Función de consulta encargada de buscar y retornar la información solicitada desde la base de datos.
    public Forecast? RetrieveActiveByBuyerMonth(int buyerId, int month, int year)
    {
        var op = new Operation { ProcedureName = "RET_ACTIVE_BY_BUYER_MONTH_FORECAST_PR" };
        op.AddIntParameter("@BuyerId", buyerId);
        op.AddIntParameter("@Month", month);
        op.AddIntParameter("@Year", year);
        var results = sqlDao.ExecuteQueryProcedure(op);
        return results.Count > 0 ? BuildForecast(results[0]) : null;
    }

    // Función encargada de modificar y actualizar los campos operacionales de registros existentes.
    public void UpdateStatus(int id, string status, DateTime updated)
    {
        var op = new Operation { ProcedureName = "UPD_STATUS_FORECAST_PR" };
        op.AddIntParameter("@Id", id);
        op.AddStringParameter("@Status", status);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public void BlockMonth(int month, int year, DateTime updated)
    {
        var op = new Operation { ProcedureName = "BLOCK_MONTH_FORECAST_PR" };
        op.AddIntParameter("@Month", month);
        op.AddIntParameter("@Year", year);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public void CancelBeyond3Months(int buyerId, int startMonth, int startYear, DateTime updated)
    {
        var op = new Operation { ProcedureName = "CANCEL_BEYOND_3M_FORECAST_PR" };
        op.AddIntParameter("@BuyerId", buyerId);
        op.AddIntParameter("@StartMonth", startMonth);
        op.AddIntParameter("@StartYear", startYear);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedure(op);
    }

    // --- Overloads transaccionales (§37.25, ciclo ACID de Distribución Comercial) ---

    public void UpdateStatus(int id, string status, DateTime updated, SqlConnection conn, SqlTransaction tx)
    {
        var op = new Operation { ProcedureName = "UPD_STATUS_FORECAST_PR" };
        op.AddIntParameter("@Id", id);
        op.AddStringParameter("@Status", status);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedureInTransaction(op, conn, tx);
    }

    public void BlockMonth(int month, int year, DateTime updated, SqlConnection conn, SqlTransaction tx)
    {
        var op = new Operation { ProcedureName = "BLOCK_MONTH_FORECAST_PR" };
        op.AddIntParameter("@Month", month);
        op.AddIntParameter("@Year", year);
        op.AddDateTimeParameter("@Updated", updated);
        sqlDao.ExecuteProcedureInTransaction(op, conn, tx);
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private static Forecast BuildForecast(Dictionary<string, object> row) => new()
    {
        Id = (int)row["Id"],
        BuyerId = (int)row["BuyerId"],
        Month = (int)row["Month"],
        Year = (int)row["Year"],
        AmountMWh = (decimal)row["AmountMWh"],
        Status = (string)row["Status"],
        Created = (DateTime)row["Created"],
        Updated = row["Updated"] as DateTime? ?? default
    };
}
