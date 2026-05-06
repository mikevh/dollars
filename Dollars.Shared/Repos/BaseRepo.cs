using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public abstract class BaseRepo
{
    protected readonly string _connectionString;

    public BaseRepo(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? throw new ArgumentException("ConnectionStrings:DefaultConnection not found");
    }
    
    public async Task<DbTransaction> BeginTransactionAsync()
    {
        var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var rv = await conn.BeginTransactionAsync();

        return rv;
    }

    protected async Task<T> WithConnAsync<T>(IDbTransaction? trans, Func<SqlConnection, Task<T>> action)
    {
        if (trans?.Connection is SqlConnection existing)
        {
            return await action(existing);
        }

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var rv = await action(conn);
        return rv;
    }

    protected async Task WithConnectionAsync(IDbTransaction? trans, Func<SqlConnection, Task> action)
    {
        if (trans?.Connection is SqlConnection existing)
        {
            await action(existing);
            return;
        }

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await action(conn);
    }
}