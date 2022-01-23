using Microsoft.Extensions.DependencyInjection;
using System;

namespace StatementDeserializer;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStatementDeserializer(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        services.AddSingleton<IStatementDeserializer, CardAccountStatementDeserializer>();
        services.AddSingleton<IStatementDeserializer, DepositAccountStatementDeserializer>();
        services.AddSingleton<IStatementFactory, StatementFactory>();
        return services;
    }
}