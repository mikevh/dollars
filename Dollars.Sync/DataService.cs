public class DataService
{
    private readonly AccountsRepo _accountsRepo;

    public DataService(AccountsRepo accountsRepo)
    {
        _accountsRepo = accountsRepo;
    }

    public async Task Save(SyncResult data)
    {
        var trans = await _accountsRepo.BeginTransactionAsync();
        var accountIds = new Dictionary<string, int>();

        try
        {
            // account
            foreach(var a in data.Accounts)
            {
                var id = await _accountsRepo.EnsureAccountAsync(a, trans);
                accountIds[a.SourceId] = id;
            }

            // balances
            foreach(var b in data.AccountBalances)
            {
                b.Value.AccountId = accountIds[b.Key];
                
                await _accountsRepo.SaveBalanceAsync(b.Value, trans);
            }
            
            // transactions
            foreach(var kvp in data.Transactions)
            {
                var accountId = accountIds[kvp.Key];
                kvp.Value.ForEach(t => t.AccountId = accountId);
                await _accountsRepo.SaveTransactionsAsync(kvp.Value, trans);
            }

            await trans.CommitAsync();
        }
        catch(Exception)
        {
            await trans.RollbackAsync();
            throw;
        }
    }
}