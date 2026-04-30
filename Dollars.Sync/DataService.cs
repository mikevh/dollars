public class DataService
{
    private readonly AccountsRepo _accountsRepo;

    public DataService(AccountsRepo accountsRepo)
    {
        _accountsRepo = accountsRepo;
    }

    public async Task Save(SyncResult data)
    {
        var accountIds = new Dictionary<string, int>();

        // account
        foreach(var a in data.Accounts)
        {
            var id = await _accountsRepo.EnsureAccountAsync(a);
            accountIds[a.SourceId] = id;
        }

        // balances
        foreach(var b in data.AccountBalances)
        {
            b.Value.AccountId = accountIds[b.Key];
            
            var latestBalance = await _accountsRepo.LatestBalanceForAccountAsync(b.Value.AccountId);
            if(latestBalance != null && latestBalance.Date > latestBalance.Date.AddHours(12)) // todo: make the time a setting
            {
                await _accountsRepo.SaveBalance(b.Value);                
            }
        }
        
        // transactions
        foreach(var kvp in data.Transactions)
        {
            var accountId = accountIds[kvp.Key];
            var transactions = kvp.Value;
            transactions.ForEach(t => t.AccountId = accountId);
            await _accountsRepo.SaveTransactionsAsync(transactions);
        }
    }
}