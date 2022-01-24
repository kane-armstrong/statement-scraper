using Microsoft.Extensions.DependencyInjection;

namespace StatementDeserializer;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStatementDeserializer(this IServiceCollection services)
    {
        services.AddSingleton<IStatementDeserializer, CardAccountStatementDeserializer>();
        services.AddSingleton<IStatementDeserializer, DepositAccountStatementDeserializer>();
        services.AddSingleton<IStatementFactory, StatementFactory>();
        return services;
    }
}