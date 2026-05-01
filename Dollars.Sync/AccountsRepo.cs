using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data.Common;
using System.Data;

public class AccountsRepo
{
    private readonly string _connectionString;

    public AccountsRepo(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? throw new Exception("Connection string 'DefaultConnection' not found.");
    }

    public async Task<DbTransaction> BeginTransactionAsync()
    {
        var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var rv = await conn.BeginTransactionAsync();

        return rv;
    }

    public async Task<Account?> GetByIdAsync(int Id, IDbTransaction? trans = default)
    {
        var sql = "select * from Accounts where Id = @Id"; // todo: don't select *
        var rv = await WithConnAsync(trans, c => c.QueryFirstOrDefaultAsync<Account>(sql, new { Id }, trans));

        return rv;
    }

    public async Task<int> EnsureAccountAsync(Account account, IDbTransaction? trans = default)
    {
        var rv = await WithConnAsync(trans, async c =>
        {
            var sql = "select Id from Accounts where SourceId = @SourceId and UserId = @UserId";
            var id = await c.QueryFirstOrDefaultAsync<int>(sql, account, trans);

            if (id != 0) 
            {
                return id;
            }

            sql = "insert Accounts(UserId, SourceId, Name, CreatedOn, UpdatedOn) values(@UserId, @SourceId, @Name, getutcdate(), getutcdate()); select cast(scope_identity() as int)";
            return await c.ExecuteScalarAsync<int>(sql, account, trans);
        });

        return rv;
    }

    public async Task SaveBalanceAsync(AccountBalance balance, IDbTransaction? trans = null)
    {
        var sql = "insert AccountBalances (AccountId, Balance, Date, CreatedOn, UpdatedOn) values (@AccountId, @Balance, @Date, getutcdate(), getutcdate())";
        await WithConnAsync(trans, c => c.ExecuteAsync(sql, balance, trans));
    }

    public async Task SaveTransactionsAsync(IEnumerable<Transaction> txs, IDbTransaction? trans = null)
    {
        await WithConnectionAsync(trans, async c =>
        {
            var sql = "select SourceId from Transactions where SourceId in @SourceIds";
            var existing = await c.QueryAsync<string>( sql, new { SourceIds = txs.Select(t => t.SourceId) }, trans);

            sql = "insert Transactions (AccountId, SourceId, Payee, Date, Amount, Description, Memo, CreatedOn, UpdatedOn) values (@AccountId, @SourceId, @Payee, @Date, @Amount, @Description, @Memo, getutcdate(), getutcdate())";
            await c.ExecuteAsync(sql, txs.Where(t => !existing.Contains(t.SourceId)), trans);
        });
    }

    public async Task<SyncLog?> LatestSyncLogForProviderAsync(string providerName, IDbTransaction? trans = null)
    {
        var sql = "select top 1 * from SyncLogs where Provider = @providerName order by SyncDate desc"; // todo: don't select *
        var rv = await WithConnAsync(trans, c => c.QueryFirstOrDefaultAsync<SyncLog>(sql, new { providerName }, trans));

        return rv;
    }

    public async Task SaveSyncLogAsync(SyncLog syncLog, IDbTransaction? trans = null)
    {
        var sql = "insert SyncLogs (SyncDate, Provider, Success, ErrorMessage, TransactionCount, JsonData, CreatedOn, UpdatedOn) values (@SyncDate, @Provider, @Success, @ErrorMessage, @TransactionCount, @JsonData, getutcdate(), getutcdate())";
        await WithConnAsync(trans, conn => conn.ExecuteAsync(sql, syncLog, trans));
    }

    private async Task<T> WithConnAsync<T>(IDbTransaction? trans, Func<SqlConnection, Task<T>> action)
    {
        if (trans?.Connection is SqlConnection existing)
            return await action(existing);

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var rv = await action(conn);
        return rv;
    }

    private async Task WithConnectionAsync(IDbTransaction? trans, Func<SqlConnection, Task> action)
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
