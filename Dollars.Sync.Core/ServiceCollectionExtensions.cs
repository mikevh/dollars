using Dollars.Shared.Repos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dollars.Sync.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSyncCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SimpleFinSettings>(configuration.GetSection("SimpleFin"));
        services.Configure<PlaidSettings>(configuration.GetSection("Plaid"));

        services.AddHttpClient();

        services.AddSingleton<IFinancialDataProvider, SimpleFin>();
        services.AddSingleton<IFinancialDataProvider, Plaid>();
        services.AddSingleton<AccountsRepo>();
        services.AddSingleton<DataService>();
        services.AddSingleton<SyncOrchestrator>();

        return services;
    }
}
