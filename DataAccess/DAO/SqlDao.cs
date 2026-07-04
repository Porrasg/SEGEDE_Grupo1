using Microsoft.Data.SqlClient;
using System.Data;

namespace SEGEDE_Grupo1.DataAccess.DAO;

/// <summary>
/// Singleton DAO para ejecución de Stored Procedures contra Azure SQL Database (§11.1).
/// Connection string con Encrypt=True, ConnectRetryCount=3, ConnectRetryInterval=10.
/// </summary>
public class SqlDao
{
    private static SqlDao? _instance;
    private readonly string _connectionString;

    private SqlDao()
    {
        // Lee connection string de la configuración de la aplicación.
        // En producción se configura desde appsettings.json vía WebAPI/Program.cs.
        _connectionString = ConnectionStringHolder.ConnectionString
            ?? throw new InvalidOperationException(
                "Connection string not configured. Call SqlDao.Configure() during startup.");
    }

    /// <summary>
    /// Obtiene la instancia Singleton del SqlDao.
    /// </summary>
    public static SqlDao GetInstance() => _instance ??= new SqlDao();

    /// <summary>
    /// Configura la cadena de conexión antes de usar el DAO.
    /// Debe llamarse una sola vez durante el startup de la aplicación.
    /// </summary>
    public static void Configure(string connectionString)
    {
        ConnectionStringHolder.ConnectionString = connectionString;
        _instance = null; // Fuerza recreación con nueva cadena
    }

    /// <summary>
    /// Ejecuta un SP que no retorna resultados (INSERT, UPDATE, DELETE).
    /// Retorna el número de filas afectadas.
    /// </summary>
    public int ExecuteProcedure(Operation operation)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        using var cmd = CreateCommand(operation, conn);
        return cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Ejecuta un SP que retorna un result set.
    /// Cada fila se mapea a un Dictionary&lt;string, object&gt;.
    /// </summary>
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

    /// <summary>
    /// Abre y retorna una conexión para operaciones transaccionales ACID.
    /// El caller es responsable de cerrar/disponer la conexión.
    /// </summary>
    public SqlConnection GetOpenConnection()
    {
        var conn = new SqlConnection(_connectionString);
        conn.Open();
        return conn;
    }

    /// <summary>
    /// Ejecuta un SP dentro de una transacción existente (INSERT, UPDATE, DELETE).
    /// </summary>
    public int ExecuteProcedureInTransaction(Operation operation, SqlConnection conn, SqlTransaction tx)
    {
        using var cmd = CreateCommand(operation, conn, tx);
        return cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Ejecuta un SP que retorna resultados dentro de una transacción existente.
    /// </summary>
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

/// <summary>
/// Holder estático para la cadena de conexión, configurada durante el startup.
/// </summary>
internal static class ConnectionStringHolder
{
    internal static string? ConnectionString { get; set; }
}
