using Dollars.Sync.Core;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .CreateLogger();

try
{
    Log.Information("app started");

    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((_, services) =>
        {
            services.AddSingleton<IConfiguration>(config);
            services.AddSyncCore(config);
        })
        .Build();

    var orchestrator = host.Services.GetRequiredService<SyncOrchestrator>();
    await orchestrator.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "app crashed");
}
finally
{
    Log.CloseAndFlush();
}
