public interface IFinancialDataProvider
{
    Task<SyncResult> GetTransactionsAsync(CancellationToken cancellationToken = default);
}
