using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>()
    .Build();

var cs = config.GetConnectionString("DefaultConnection");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.Configure<SimpleFinSettings>(config.GetSection("SimpleFin"));
        services.Configure<PlaidSettings>(config.GetSection("Plaid"));

        services.AddHttpClient();

        services.AddSingleton<DBSettings>();
        services.AddScoped<IFinancialDataProvider, SimpleFin>();
        services.AddScoped<IFinancialDataProvider, Plaid>();
        services.AddScoped<AccountsRepo>();
    })
    .Build();

var providers = host.Services.GetRequiredService<IEnumerable<IFinancialDataProvider>>();

try
{
    foreach(var p in providers)
    {
        var result = await p.GetTransactionsAsync();
        Console.WriteLine($"Provider: {p.GetType().Name}");
        Console.WriteLine($"Accounts: {result.Accounts.Count}, Transactions: {result.Transactions.Count}, Errors: {result.Errors.Count}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

