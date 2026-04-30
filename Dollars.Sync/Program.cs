using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>()
    .Build();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton<IConfiguration>(config);
        services.Configure<SimpleFinSettings>(config.GetSection("SimpleFin"));
        services.Configure<PlaidSettings>(config.GetSection("Plaid"));

        services.AddHttpClient();

        services.AddSingleton<IFinancialDataProvider, SimpleFin>();
        services.AddSingleton<IFinancialDataProvider, Plaid>();
        services.AddSingleton<AccountsRepo>();
        services.AddSingleton<DataService>();
    })
    .Build();

var providers = host.Services.GetRequiredService<IEnumerable<IFinancialDataProvider>>();
var dataService = host.Services.GetRequiredService<DataService>();
var repo = host.Services.GetRequiredService<AccountsRepo>();

foreach(var p in providers)
{
    SyncResult result;
    try
    {
        if(!await p.ReadyToSync())
        {
            continue;
        }
        result = await p.GetTransactionsAsync();
        Console.WriteLine($"Provider: {p.GetType().Name}");
        Console.WriteLine($"Accounts: {result.Accounts.Count}, Transactions: {result.Transactions.Values.Sum(t => t.Count)}, Errors: {result.Errors.Count}");
        await dataService.Save(result);            
    }
    catch(Exception ex)
    {
        Console.WriteLine($"Error syncing with provider {p.GetType().Name}: {ex.Message}");
        result = new SyncResult
        {
            Errors = new List<string> { $"Exception during sync: {ex.Message}" }
        };
    }

    // todo: this shouldn't be here
    // todo: the transactioncount should be aware of how many were new
    // todo: check for updated transactions? same id, but updated data?

    await repo.SaveSyncLogAsync(new SyncLog
    {
        SyncDate = DateTime.UtcNow,
        Provider = p.ProviderName,
        Success = result.Errors.Count == 0,
        ErrorMessage = string.Join("; ", result.Errors),
        TransactionCount = result.Transactions.Values.Sum(t => t.Count),
        CreatedOn = DateTime.UtcNow,
        UpdatedOn = DateTime.UtcNow
    });
}

