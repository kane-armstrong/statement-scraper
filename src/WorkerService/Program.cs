using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using StatementDeserializer;
using StatementSaver;
using StatementScraper;
using System.IO;
using WorkerService;

await new HostBuilder()
    .ConfigureAppConfiguration((_, builder) =>
    {
        builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddUserSecrets<Program>();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        services.Configure<SqlConnectionFactoryOptions>(configuration.GetSection(nameof(SqlConnectionFactoryOptions)));
        services.Configure<BankStatementWebScraperOptions>(configuration.GetSection(nameof(BankStatementWebScraperOptions)));
        services.Configure<TransactionEtlOptions>(configuration.GetSection(nameof(TransactionEtlOptions)));

        services.AddStatementDeserializer();
        services.AddStatementSaver();
        services.AddStatementScraper();

        services.AddScoped<AccountEtl>();
        services.AddScoped<TransactionEtl>();
        services.AddScoped<EtlRunner>();

        services.AddLogging(options =>
        {
            options.AddSerilog(dispose: true);
        });

        services.AddHostedService<Worker>();
    })
    .RunConsoleAsync();