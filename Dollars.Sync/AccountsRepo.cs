using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data.Common;
using System.Data;

public class AccountsRepo : IDisposable
{
    private string _connectionString;
    private SqlConnection? _sqlConnection;

    public AccountsRepo(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? throw new Exception("Connection string 'DefaultConnection' not found.");
    }

    public async Task<DbTransaction> BeginTransactionAsync()
    {
        var conn = await GetConnectionAsync();
        var trans = await conn.BeginTransactionAsync();
        return trans;
    }

    public async Task<Account?> GetByIdAsync(int id, DbTransaction? trans = default)
    {
        var conn = trans?.Connection ?? await GetConnectionAsync();

        var sql = "select * from Accounts where Id = @Id";
        var account = await conn.QueryFirstOrDefaultAsync<Account>(sql, new { Id = id }, trans);

        return account;
    }

    public async Task<int> EnsureAccountAsync(Account account, DbTransaction? trans = default)
    {
        var conn = trans?.Connection ?? await GetConnectionAsync();        

        var sql = "select Id from Accounts where SourceId = @SourceId and UserId = @UserId";
        var id = await conn.QueryFirstOrDefaultAsync<int>(sql, account, trans);
        
        if(id != 0)
        {
            return id;
        }

        sql = "insert Accounts(UserId, SourceId, Name, CreatedOn, UpdatedOn) values(@UserId, @SourceId, @Name, getutcdate(), getutcdate()); select cast(scope_identity() as int)";
        id = await conn.ExecuteScalarAsync<int>(sql, account, trans);
        
        return id;
    }

    public async Task SaveBalanceAsync(AccountBalance balance, IDbTransaction? trans = null)
    {
        var conn = trans?.Connection ?? await GetConnectionAsync();
        
        var sql = "insert AccountBalances (AccountId, Balance, Date, CreatedOn, UpdatedOn) values (@AccountId, @Balance, @Date, getutcdate(), getutcdate())";;
        await conn.ExecuteAsync(sql, balance, trans);
    }

    public async Task SaveTransactionsAsync(IEnumerable<Transaction> transactions, IDbTransaction? trans = null)
    {
        var conn = trans?.Connection ?? await GetConnectionAsync();
        
        var existingSourceIds = await conn.QueryAsync<string>("select SourceId from Transactions where SourceId in @SourceIds", 
            new { SourceIds = transactions.Select(t => t.SourceId) }, trans);
    
        var sql = "insert Transactions (AccountId, SourceId, Payee, Date, Amount, Description, Memo, CreatedOn, UpdatedOn) values (@AccountId, @SourceId, @Payee, @Date, @Amount, @Description, @Memo, getutcdate(), getutcdate())";
        await conn.ExecuteAsync(sql, transactions.Where(t => !existingSourceIds.Contains(t.SourceId)), trans);
    }   

    public async Task<SyncLog?> LatestSyncLogForProviderAsync(string providerName, IDbTransaction? trans = null)
    {
        var conn = trans?.Connection ?? await GetConnectionAsync();

        var sql = "select top 1 * from SyncLogs where Provider = @providerName order by SyncDate desc";
        var rv = await conn.QueryFirstOrDefaultAsync<SyncLog>(sql, new { providerName }, trans);

        return rv;
    }

    public async Task SaveSyncLogAsync(SyncLog syncLog, IDbTransaction? trans = null)
    {
        var conn = trans?.Connection ?? await GetConnectionAsync();
        
        var sql = "insert SyncLogs (SyncDate, Provider, Success, ErrorMessage, TransactionCount, JsonData, CreatedOn, UpdatedOn) values (@SyncDate, @Provider, @Success, @ErrorMessage, @TransactionCount, @JsonData, getutcdate(), getutcdate())";
        await conn.ExecuteAsync(sql, syncLog, trans);
    }

    protected async Task<SqlConnection> GetConnectionAsync()
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