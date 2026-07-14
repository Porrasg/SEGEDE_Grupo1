using Microsoft.Data.SqlClient;
using System.Data;

namespace SEGEDE_Grupo1.DataAccess.DAO;

// Singleton DAO para ejecución de Stored Procedures contra Azure SQL Database (§11.1).
// Connection string con Encrypt=True, ConnectRetryCount=3, ConnectRetryInterval=10.
public class SqlDao
{
    private static SqlDao? _instance;
    private readonly string _connectionString;

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private SqlDao()
    {
        // Lee connection string de la configuración de la aplicación.
        // En producción se configura desde appsettings.json vía WebAPI/Program.cs.
        _connectionString = ConnectionStringHolder.ConnectionString
            ?? throw new InvalidOperationException(
                "Connection string not configured. Call SqlDao.Configure() during startup.");
    }

    // Obtiene la instancia Singleton del SqlDao.
    public static SqlDao GetInstance() => _instance ??= new SqlDao();

    // Configura la cadena de conexión antes de usar el DAO.
    // Debe llamarse una sola vez durante el startup de la aplicación.
    public static void Configure(string connectionString)
    {
        ConnectionStringHolder.ConnectionString = connectionString;
        _instance = null; // Fuerza recreación con nueva cadena
    }

    // Ejecuta un SP que no retorna resultados (INSERT, UPDATE, DELETE).
    // Retorna el número de filas afectadas.
    public int ExecuteProcedure(Operation operation)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        using var cmd = CreateCommand(operation, conn);
        return cmd.ExecuteNonQuery();
    }

    // Ejecuta un SP que retorna un result set.
    // Cada fila se mapea a un Dictionary&lt;string, object&gt;.
    public List<Dictionary<string, object>> ExecuteQueryProcedure(Operation operation)
    {
        var results = new List<Dictionary<string, object>>();

        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        using var cmd = CreateCommand(operation, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
            }
            results.Add(row);
        }

        return results;
    }

    // Abre y retorna una conexión para operaciones transaccionales ACID.
    // El caller es responsable de cerrar/disponer la conexión.
    public SqlConnection GetOpenConnection()
    {
        var conn = new SqlConnection(_connectionString);
        conn.Open();
        return conn;
    }

    // Ejecuta un SP dentro de una transacción existente (INSERT, UPDATE, DELETE).
    public int ExecuteProcedureInTransaction(Operation operation, SqlConnection conn, SqlTransaction tx)
    {
        using var cmd = CreateCommand(operation, conn, tx);
        return cmd.ExecuteNonQuery();
    }

    // Ejecuta un SP que retorna resultados dentro de una transacción existente.
    public List<Dictionary<string, object>> ExecuteQueryInTransaction(
        Operation operation, SqlConnection conn, SqlTransaction tx)
    {
        var results = new List<Dictionary<string, object>>();

        using var cmd = CreateCommand(operation, conn, tx);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
            }
            results.Add(row);
        }

        return results;
    }

    // Función encargada de registrar e insertar nuevos elementos en el almacén de datos cumpliendo las reglas de negocio.
    private static SqlCommand CreateCommand(
        Operation operation, SqlConnection conn, SqlTransaction? tx = null)
    {
        var cmd = new SqlCommand(operation.ProcedureName, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (tx != null) cmd.Transaction = tx;

        foreach (var param in operation.Parameters)
        {
            cmd.Parameters.Add(param);
        }

        return cmd;
    }
}

// Holder estático para la cadena de conexión, configurada durante el startup.
internal static class ConnectionStringHolder
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    internal static string? ConnectionString { get; set; }
}
