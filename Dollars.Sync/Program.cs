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

try
{
    foreach(var p in providers)
    {
        var result = await p.GetTransactionsAsync();
        Console.WriteLine($"Provider: {p.GetType().Name}");
        Console.WriteLine($"Accounts: {result.Accounts.Count}, Transactions: {result.Transactions.Count}, Errors: {result.Errors.Count}");

        await dataService.Save(result);

    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

