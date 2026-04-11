using Microsoft.Data.SqlClient;

namespace DataVisualiser.Core.Data.Repositories;

internal abstract class DataFetcherQueryGroup
{
    private readonly string _connectionString;

    protected DataFetcherQueryGroup(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected async Task<SqlConnection> OpenConnectionAsync()
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}
