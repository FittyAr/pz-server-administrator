using Microsoft.Data.Sqlite;
using System.Data;

namespace pz_server_administrator.Services;

public class SqliteService : ISqliteService
{
    private readonly ILogger<SqliteService> _logger;

    public SqliteService(ILogger<SqliteService> logger)
    {
        _logger = logger;
    }

    public async Task<List<string>> GetTablesAsync(string dbPath)
    {
        var tables = new List<string>();
        if (!File.Exists(dbPath)) return tables;

        var connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    public async Task<DataTable> QueryTableAsync(string dbPath, string tableName)
    {
        var dataTable = new DataTable();
        if (!File.Exists(dbPath)) return dataTable;

        var connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {tableName} LIMIT 1000;"; // Safety limit

        using var reader = await command.ExecuteReaderAsync();
        dataTable.Load(reader);

        return dataTable;
    }

    public async Task ExecuteNonQueryAsync(string dbPath, string query, Dictionary<string, object> parameters)
    {
        var connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = query;

        foreach (var param in parameters)
        {
            command.Parameters.AddWithValue(param.Key, param.Value);
        }

        await command.ExecuteNonQueryAsync();
    }
}
