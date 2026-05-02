using Dollars.Shared.Models;

public class SyncResult
{
    public List<Account> Accounts { get; set; } = [];
    public Dictionary<string, AccountBalance> AccountBalances { get; set; } = []; // maps account source id to balance
    public Dictionary<string, List<Transaction>> Transactions { get; set; } = []; // maps account source id to transactions
    public List<string> Errors { get; set; } = [];
    public bool HasMore { get; set; }
}
