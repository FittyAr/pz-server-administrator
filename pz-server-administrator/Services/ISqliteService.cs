using System.Data;

namespace pz_server_administrator.Services;

public interface ISqliteService
{
    Task<List<string>> GetTablesAsync(string dbPath);
    Task<DataTable> QueryTableAsync(string dbPath, string tableName);
    Task ExecuteNonQueryAsync(string dbPath, string query, Dictionary<string, object> parameters);
}
