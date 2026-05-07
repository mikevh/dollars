using Dollars.Shared.Repos;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dollars.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class SimpleFin : IFinancialDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly SimpleFinSettings _settings;
    private readonly ILogger<SimpleFin> _logger;
    private readonly AccountsRepo _repo;
    public string ProviderName => "SimpleFIN";

    public SimpleFin(ILogger<SimpleFin> logger, HttpClient httpClient, IOptions<SimpleFinSettings> settings, AccountsRepo repo)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        _repo = repo;
    }

    public async Task<bool> ReadyToSync()
    {
        if(!_settings.Enabled) return false;

        var latestSync = await _repo.LatestSyncLogForProviderAsync(ProviderName);
        var lastSyncDate = latestSync?.SyncDate ?? DateTime.MinValue;

        if(DateTime.UtcNow > lastSyncDate + TimeSpan.FromHours(_settings.WaitHours))
        {
            return true;
        }

        return false;
    }

    public async Task<SyncResult> GetTransactionsAsync(CancellationToken cancellationToken = default)
    {
        var body = await QueryServiceAsync(cancellationToken);
        var data = JsonSerializer.Deserialize<SimplefinAccountsResponse>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });

        var rv = new SyncResult();

        if(data == null)
        {
            _logger.LogError("Failed to parse SimpleFIN response: {Body}", body);
            rv.Errors.Add($"Failed to parse SimpleFIN response: {body}");
            return rv;
        }

        if(data.Errors != null && data.Errors.Count > 0)
        {
            foreach(var error in data.Errors)
            {
                _logger.LogError("SimpleFIN API error: {Error}", error);
                rv.Errors.Add($"SimpleFIN API error: {error}");
            }
        }

        // todo: write sync log here

        foreach(var a in data.Accounts ?? [])
        {            
            rv.Accounts.Add(new Account
            {
                SourceId = a.Id,
                Name = a.Name,
            });

            rv.AccountBalances.Add(a.Id, new AccountBalance
            {
                Balance = decimal.TryParse(a.Balance, out var b) ? b : 0,
                Date = DateTimeOffset.FromUnixTimeSeconds(a.BalanceDate).DateTime,
            });
            
            rv.Transactions.Add(a.Id, a.Transactions?.Select(t => new Transaction
                {
                    SourceId = t.Id,
                    Payee = t.Payee ?? "",
                    Amount = decimal.TryParse(t.Amount, out var d) ? d : 0,
                    Date = DateTimeOffset.FromUnixTimeSeconds(t.Posted).DateTime,
                    Description = t.Description ?? "",
                    Memo = t.Memo ?? "",
                }).ToList() ?? []);            
        }

        return rv;
    }

    private async Task<string> QueryServiceAsync(CancellationToken cancellationToken = default)
    {
        var (accountsUrl, username, password) = ParseAccessUrl(_settings.AccessUrl);
        var endDate = DateTimeOffset.UtcNow;
        var startDate = endDate.AddDays(-1 * _settings.SyncBackDays);

        var latestSync = await _repo.LatestSyncLogForProviderAsync(ProviderName);
        endDate = latestSync == null ? 
                    endDate.AddDays(_settings.SyncBackDays) : 
                    new DateTimeOffset(latestSync.SyncDate).AddHours(-2);
        // todo: something is off here.... keeps getting the 45 day warning...
        var url = $"{accountsUrl}?start-date={startDate.ToUnixTimeSeconds()}&end-date={endDate.ToUnixTimeSeconds()}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if(!response.IsSuccessStatusCode)
        {
            _logger.LogError("SimpleFIN returned {StatusCode}", response.StatusCode);
        }

        return body;
    }

    private static (string accountsUrl, string username, string password) ParseAccessUrl(string accessUrl)
    {
        var uri = new Uri(accessUrl);
        var colonIdx = uri.UserInfo.IndexOf(':');
        var username = Uri.UnescapeDataString(uri.UserInfo[..colonIdx]);
        var password = Uri.UnescapeDataString(uri.UserInfo[(colonIdx + 1)..]);
        var accountsUrl = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}/accounts";
        return (accountsUrl, username, password);
    }
}

// SimpleFIN API response models
public class SimplefinAccountsResponse
{
    [JsonPropertyName("errors")]
    public List<string>? Errors { get; set; }

    [JsonPropertyName("accounts")]
    public List<SimplefinAccount>? Accounts { get; set; }
}

public class SimplefinAccount
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";

    [JsonPropertyName("balance")]
    public string Balance { get; set; } = "0";

    [JsonPropertyName("available-balance")]
    public string? AvailableBalance { get; set; }

    [JsonPropertyName("balance-date")]
    public long BalanceDate { get; set; }

    [JsonPropertyName("transactions")]
    public List<SimplefinTransaction>? Transactions { get; set; }
}

public class SimplefinTransaction
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("posted")]
    public long Posted { get; set; }

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = "0";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("payee")]
    public string? Payee { get; set; }

    [JsonPropertyName("memo")]
    public string? Memo { get; set; }

    [JsonPropertyName("transacted_at")]
    public long? TransactedAt { get; set; }
}
