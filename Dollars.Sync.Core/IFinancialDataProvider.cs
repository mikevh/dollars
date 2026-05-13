namespace Dollars.Sync.Core;

public interface IFinancialDataProvider
{
    Task<SyncResult> GetTransactionsAsync(CancellationToken cancellationToken = default);
    string ProviderName { get; }
    Task<bool> ReadyToSync();
}
