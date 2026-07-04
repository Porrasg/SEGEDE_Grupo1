using Microsoft.Data.SqlClient;

namespace SEGEDE_Grupo1.DataAccess.DAO;

// Encapsula nombre de Stored Procedure y sus parámetros SQL (§11.2).
public class Operation
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string ProcedureName { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public List<SqlParameter> Parameters { get; } = new();

    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public void AddStringParameter(string name, string? value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.NVarChar)
        {
            Value = (object?)value ?? DBNull.Value
        });
    }

    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public void AddIntParameter(string name, int value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.Int) { Value = value });
    }

    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public void AddNullableIntParameter(string name, int? value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.Int)
        {
            Value = (object?)value ?? DBNull.Value
        });
    }

    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public void AddDecimalParameter(string name, decimal value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.Decimal)
        {
            Precision = 18,
            Scale = 4,
            Value = value
        });
    }

    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public void AddNullableDecimalParameter(string name, decimal? value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.Decimal)
        {
            Precision = 18,
            Scale = 4,
            Value = (object?)value ?? DBNull.Value
        });
    }

    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public void AddDateTimeParameter(string name, DateTime value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.DateTime2) { Value = value });
    }

    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public void AddNullableDateTimeParameter(string name, DateTime? value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.DateTime2)
        {
            Value = (object?)value ?? DBNull.Value
        });
    }

    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public void AddBoolParameter(string name, bool value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.Bit) { Value = value });
    }

    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public void AddTimeParameter(string name, TimeSpan value)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.Time) { Value = value });
    }

    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    public void AddOutputIntParameter(string name)
    {
        Parameters.Add(new SqlParameter(name, System.Data.SqlDbType.Int)
        {
            Direction = System.Data.ParameterDirection.Output
        });
    }
}
