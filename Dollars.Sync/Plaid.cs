using Microsoft.Extensions.Options;

public class PlaidSettings
{
    public bool Enabled { get; set; } = false;
    public string ClientId { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}

public class Plaid : IFinancialDataProvider
{
    private readonly PlaidSettings _settings;

    public Plaid(IOptions<PlaidSettings> settings)
    {
        _settings = settings.Value;
    }

    public string ProviderName => "Plaid";

    public async Task<SyncResult> GetTransactionsAsync(CancellationToken cancellationToken = default)
    {
        return new SyncResult
        {
            Errors = new List<string> { "Plaid sync is disabled in settings." }
        };
    }

    public Task<bool> ReadyToSync()
    {
        return Task.FromResult(false);
    }
}