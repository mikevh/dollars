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
        var userId = 1; // TODO: get from context
        var conn = GetConnection();
        await conn.OpenAsync();

        var sql = "select Id from Accounts where SourceId = @SourceId";
        var existingId = await conn.QueryFirstOrDefaultAsync<int>(sql, account);

        if(existingId != 0)
        {
            return existingId;
        }
        sql = "insert Accounts(UserId, SourceId, Name, CreatedOn, UpdatedOn) values(@UserId, @SourceId, @Name, @CreatedOn, @UpdatedOn); select cast(scope_identity() as int)";

        return 0;
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