using Microsoft.Data.SqlClient;
using Dapper;

public class AccountsRepo : IDisposable
{
    private readonly DBSettings _dbSettings;
    private SqlConnection? _sqlConnection;

    public AccountsRepo(DBSettings dbSettings)
    {
        _dbSettings = dbSettings;
    }

    public async Task<int> EnsureAccountAsync(Account account)
    {
        var conn = GetConnection();
        await conn.OpenAsync();

        var sql = "select Id from Accounts where SourceAccountId = @SourceId";
        var existingId = await conn.QueryFirstOrDefaultAsync<int>(sql, new { SourceId = account.SourceId });

        if(existingId != 0)
        {
            return existingId;
        }
        
    }

    protected SqlConnection GetConnection()
    {
        _sqlConnection ??= new SqlConnection(_dbSettings.ConnectionString);
        
        return _sqlConnection;
    }

    public void Dispose()
    {
        _sqlConnection?.Dispose();
    }
}