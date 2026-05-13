using Microsoft.Extensions.Logging;

namespace Dollars.Sync.Core;

public class SyncOrchestrator
{
    private readonly IEnumerable<IFinancialDataProvider> _providers;
    private readonly DataService _dataService;
    private readonly ILogger<SyncOrchestrator> _logger;

    public SyncOrchestrator(IEnumerable<IFinancialDataProvider> providers, DataService dataService, ILogger<SyncOrchestrator> logger)
    {
        _providers = providers;
        _dataService = dataService;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        foreach (var p in _providers)
        {
            SyncResult result;
            try
            {
                if (!await p.ReadyToSync()) continue;

                do
                {
                    result = await p.GetTransactionsAsync(cancellationToken);
                    _logger.LogInformation("Provider: {Provider}, Accounts: {Accounts}, Transactions: {Transactions}, Errors: {Errors}",
                        p.ProviderName, result.Accounts.Count, result.Transactions.Values.Sum(t => t.Count), result.Errors.Count);
                    await _dataService.Save(result);
                } while (result.HasMore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing with provider {Provider}", p.ProviderName);
            }
        }
    }
}
