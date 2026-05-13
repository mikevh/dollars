using Dollars.Shared.Repos;
using System.Text.Json;
using Dollars.Shared.Models;
using Going.Plaid;
using Going.Plaid.Transactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dollars.Sync.Core;

public class Plaid : IFinancialDataProvider
{
    private readonly PlaidSettings _settings;
    private readonly AccountsRepo _repo;
    private readonly ILogger _logger;
    public string ProviderName => "Plaid";

    public Plaid(IOptions<PlaidSettings> settings, AccountsRepo repo, ILogger<Plaid> logger)
    {
        _settings = settings.Value;
        _repo = repo;
        _logger = logger;
    }

    public async Task<SyncResult> GetTransactionsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("plaid::GetTransactionsAsync");
        var rv = new SyncResult();
        var latest = await _repo.LatestSyncLogForProviderAsync(ProviderName);
        // todo: check for jsondata null passed to Deserialize
        var cursor = !string.IsNullOrEmpty(latest?.JsonData) ? JsonSerializer.Deserialize<SyncLogData>(latest.JsonData).Cursor : null;

        var plaid = new PlaidClient(Going.Plaid.Environment.Production);

        var response = await plaid.TransactionsSyncAsync(new TransactionsSyncRequest
        {
            ClientId = _settings.ClientId,
            Secret = _settings.Secret,
            AccessToken = _settings.AccessToken,
            Cursor = string.IsNullOrEmpty(cursor) ? null : cursor,
            Count = 500,
        });

        await _repo.SaveSyncLogAsync(new SyncLog
        {
            SyncDate = DateTime.UtcNow,
            Provider = ProviderName,
            Success = !response.IsSuccessStatusCode,
            JsonData = JsonSerializer.Serialize(new SyncLogData { Cursor = response.NextCursor }),
            ErrorMessage = response?.Error?.ErrorMessage ?? "",
            TransactionCount = response?.Added.Count() ?? 0,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow
        });

        if (!response.IsSuccessStatusCode)
        {
            var error = response.Error;
            rv.Errors.Add($"Plaid API error: {error?.ErrorCode} - {error?.ErrorMessage}");

            if (error?.ErrorCode == "ITEM_LOGIN_REQUIRED")
            {
                rv.Errors.Add("Bank connection requires re-authentication. Re-link the item in Plaid.");
            }

            return rv;
        }

        rv.HasMore = response.HasMore;
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
#pragma warning disable CS0612 // yea it's marked as depcreated, i know but it shouldn't be
            rv.Transactions.Add(a.SourceId, response.Added.Where(x => x.AccountId == a.SourceId)
                .Select(t => new Transaction
                {
                    SourceId = t.TransactionId ?? "",
                    Payee = t.Name ?? "",
                    Amount = (t.Amount ?? 0) * -1,
                    Date = (t.Date ?? DateOnly.MinValue).ToDateTime(TimeOnly.MinValue),
                    Description = t.OriginalDescription ?? "",
                }).ToList());
#pragma warning restore CS0612
        }

        return rv;
    }

    public async Task<bool> ReadyToSync()
    {
        var rv = false;
        if(_settings.Enabled)
        {
            var latest = await _repo.LatestSyncLogForProviderAsync(ProviderName);
            if(latest == null || latest.CreatedOn + TimeSpan.FromHours(4) < DateTime.UtcNow)
            {
                rv = true;
            }
        }

        _logger.LogTrace("plaid::ReadyToSync() = {rv}", rv);
        return rv;
    }
}

public class SyncLogData
{
    public string Cursor { get; set; } = string.Empty;
}
