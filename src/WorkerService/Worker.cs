using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;

        private const int FrequencyHours = 2;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using var scope = _serviceProvider.CreateScope();

                try
                {
                    _logger.LogInformation("Running account ETL");
                    var accountEtl = scope.ServiceProvider.GetRequiredService<AccountEtl>();
                    await accountEtl.Run(stoppingToken);
                    _logger.LogInformation("Account ETL ran successfully");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Account ETL encountered an error: {e.Message}");
                }

                var handler = scope.ServiceProvider.GetRequiredService<StatementSynchronizationHandler>();
                await handler.DownloadBankTransactions(stoppingToken);

                _logger.LogInformation("Worker finished, next execution in {frequency} hours", FrequencyHours);

                await Task.Delay(TimeSpan.FromHours(FrequencyHours), stoppingToken);
            }
        }
    }
}
