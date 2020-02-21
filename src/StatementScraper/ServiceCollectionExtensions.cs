using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace StatementScraper
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStatementScraper(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Scoped<IBankStatementWebScraper, BankStatementWebScraper>());
            return services;
        }

        public static IServiceCollection AddStatementScraper(
            this IServiceCollection services,
            Action<BankStatementWebScraperOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));
            services.AddStatementScraper();
            services.Configure(setupAction);
            return services;
        }
    }
}
