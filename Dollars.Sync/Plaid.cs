using System.Text.Json;
using System.Text.Json.Serialization;
using Going.Plaid;
using Going.Plaid.Converters;
using Going.Plaid.Transactions;
using Microsoft.Extensions.Options;

public class Plaid : IFinancialDataProvider
{
    private readonly PlaidSettings _settings;
    //private readonly PlaidClient _plaidClient;
    private readonly AccountsRepo _repo;
    public string ProviderName => "Plaid";

    public Plaid(IOptions<PlaidSettings> settings, 
    //PlaidClient plaidClient, 
    AccountsRepo repo)
    {
        _settings = settings.Value;
        //_plaidClient = plaidClient;
        _repo = repo;
    }

    public async Task<SyncResult> GetTransactionsAsync(CancellationToken cancellationToken = default)
    {
        var rv = new SyncResult();
        var latest = await _repo.LatestSyncLogForProviderAsync(ProviderName);
        var cursor = JsonSerializer.Deserialize<SyncLogData>(latest?.JsonData ?? "{}")?.Cursor ?? null;

        var plaid = new PlaidClient(Going.Plaid.Environment.Production);
        
        var response = JsonSerializer.Deserialize<TransactionsSyncResponse>(
            await File.ReadAllTextAsync("plaid_sample.json", cancellationToken), 
            options: new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                }.AddPlaidConverters()
        );

        // var response = await plaid.TransactionsSyncAsync(new TransactionsSyncRequest
        // {
        //     ClientId = _settings.ClientId,
        //     Secret = _settings.Secret,
        //     AccessToken = _settings.AccessToken,
        //     Cursor = string.IsNullOrEmpty(cursor) ? null : cursor,
        //     Count = 500,
        //     ShowRawJson = true
        // });

        // if (!response.IsSuccessStatusCode)
        // {
        //     var error = response.Error;
        //     rv.Errors.Add($"Plaid API error: {error?.ErrorCode} - {error?.ErrorMessage}");

        //     if (error?.ErrorCode == "ITEM_LOGIN_REQUIRED")
        //     {
        //         rv.Errors.Add("Bank connection requires re-authentication. Re-link the item in Plaid.");
        //     }

        //     return rv;
        // }
    
        rv.Accounts = response.Accounts.Select(a => new Account
        {
            SourceId = a.AccountId,
            Name = a.Name,
        }).ToList();

        rv.AccountBalances = response.Accounts.ToDictionary(a => a.AccountId, a => new AccountBalance
        {
            Date = DateTime.UtcNow,
            Balance = a.Balances.Current ?? 0
        });

        foreach(var a in rv.Accounts)
        {
            rv.Transactions.Add(a.SourceId, response.Added.Where(x => x.AccountId == a.SourceId)
                .Select(t => new Transaction
                {
                    SourceId = t.TransactionId ?? "",
                    Payee = t.MerchantName ?? "",
                    Amount = t.Amount ?? 0,
                    Date = (t.Date ?? DateOnly.MinValue).ToDateTime(TimeOnly.MinValue),
                    Description = t.OriginalDescription ?? "",
                }).ToList());
        }

        return rv;
    }

    public Task<bool> ReadyToSync()
    {
        return Task.FromResult(true);
    }
}

public class SyncLogData
{
    public string Cursor { get; set; } = string.Empty;
}

public class PlaidSyncState
{
    public int Id { get; set; }
    public string? ItemId { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string NextCursor { get; set; } = string.Empty;
    public DateTime LastSyncAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PlaidTransaction
{
    public int Id { get; set; }
    public string PlaidTransactionId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string? AccountName { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public DateOnly? AuthorizedDate { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? MerchantName { get; set; }
    public string? PersonalFinanceCategory { get; set; }
    public bool Pending { get; set; }
    public string? PendingTransactionId { get; set; }
    public string? PaymentChannel { get; set; }
    public string? IsoCurrencyCode { get; set; }
    public string? JsonData { get; set; }
    public bool IsRemoved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}