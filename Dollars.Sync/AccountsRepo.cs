using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;

public class AccountsRepo : IDisposable
{
    private string _connectionString;
    private SqlConnection? _sqlConnection;

    public AccountsRepo(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? throw new Exception("Connection string 'DefaultConnection' not found.");
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        var conn = await GetConnection();

        var sql = "select * from Accounts where Id = @Id";
        var account = await conn.QueryFirstOrDefaultAsync<Account>(sql, new { Id = id });

        return account;
    }

    public async Task<int> EnsureAccountAsync(Account account)
    {
        var conn = await GetConnection();        

        var sql = "select Id from Accounts where SourceId = @SourceId and UserId = @UserId";
        var id = await conn.QueryFirstOrDefaultAsync<int>(sql, account);
        
        if(id != 0)
        {
            return id;
        }

        sql = "insert Accounts(UserId, SourceId, Name, CreatedOn, UpdatedOn) values(@UserId, @SourceId, @Name, getutcdate(), getutcdate()); select cast(scope_identity() as int)";
        id = await conn.ExecuteScalarAsync<int>(sql, account);
        
        return id;
    }

    public async Task SaveBalance(AccountBalance balance)
    {
        var conn = await GetConnection();
        
        var sql = "insert AccountBalances (AccountId, Balance, Date, CreatedOn, UpdatedOn) values (@AccountId, @Balance, @Date, getutcdate(), getutcdate())";;
        await conn.ExecuteAsync(sql, balance);
    }

    public async Task SaveTransactionsAsync(IEnumerable<Transaction> transactions)
    {
        var conn = await GetConnection();
        
        var existingSourceIds = await conn.QueryAsync<string>("select SourceId from Transactions where SourceId in @SourceIds", 
            new { SourceIds = transactions.Select(t => t.SourceId) });
    
        var sql = "insert Transactions (AccountId, SourceId, Payee, Date, Amount, Description, Memo, CreatedOn, UpdatedOn) values (@AccountId, @SourceId, @Payee, @Date, @Amount, @Description, @Memo, getutcdate(), getutcdate())";
        await conn.ExecuteAsync(sql, transactions.Where(t => !existingSourceIds.Contains(t.SourceId)));
    }   

    public async Task<SyncLog?> LatestSyncLogForProvider(string providerName)
    {
        var conn = await GetConnection();

        var sql = "select top 1 * from SyncLogs where Provider = @providerName order by SyncDate desc";
        var rv = await conn.QueryFirstOrDefaultAsync<SyncLog>(sql, new { providerName });

        return rv;
    }

    public async Task SaveSyncLog(SyncLog syncLog)
    {
        var conn = await GetConnection();
        
        var sql = "insert SyncLogs (SyncDate, Provider, Success, ErrorMessage, TransactionCount, CreatedOn, UpdatedOn) values (@SyncDate, @Provider, @Success, @ErrorMessage, @TransactionCount, getutcdate(), getutcdate())";
        await conn.ExecuteAsync(sql, syncLog);
    }

    protected async Task<SqlConnection> GetConnection()
    {
        _sqlConnection ??= new SqlConnection(_connectionString);
        if(_sqlConnection.State != System.Data.ConnectionState.Open)
        {
            await _sqlConnection.OpenAsync();
        }

        return _sqlConnection;
    }

    public void Dispose()
    {
        _sqlConnection?.Dispose();
    }

}