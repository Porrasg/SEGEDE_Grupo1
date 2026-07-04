using Microsoft.Data.SqlClient;

namespace SEGEDE_Grupo1.DataAccess.DAO;

/// <summary>
/// Encapsula nombre de Stored Procedure y sus parámetros SQL (§11.2).
/// </summary>
public class Operation
{
    public string ProcedureName { get; set; } = string.Empty;
    public List<SqlParameter> Parameters { get; } = new();

    public void AddStringParameter(string name, string? value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.NVarChar)
        {
            Value = (object?)value ?? DBNull.Value
        });
    }

    public void AddIntParameter(string name, int value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.Int) { Value = value });
    }

    public void AddNullableIntParameter(string name, int? value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.Int)
        {
            Value = (object?)value ?? DBNull.Value
        });
    }

    public void AddDecimalParameter(string name, decimal value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.Decimal)
        {
            Precision = 18,
            Scale = 4,
            Value = value
        });
    }

    public void AddNullableDecimalParameter(string name, decimal? value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.Decimal)
        {
            Precision = 18,
            Scale = 4,
            Value = (object?)value ?? DBNull.Value
        });
    }

    public void AddDateTimeParameter(string name, DateTime value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.DateTime2) { Value = value });
    }

    public void AddNullableDateTimeParameter(string name, DateTime? value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.DateTime2)
        {
            Value = (object?)value ?? DBNull.Value
        });
    }

    public void AddBoolParameter(string name, bool value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.Bit) { Value = value });
    }

    public void AddTimeParameter(string name, TimeSpan value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.Time) { Value = value });
    }

    public void AddOutputIntParameter(string name)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.Int)
        {
            Direction = System.Data.ParameterDirection.Output
        });
    }
}
