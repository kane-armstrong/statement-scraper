﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StatementSaver.Repositories;

namespace StatementSaver;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStatementSaver(this IServiceCollection services)
    {
        services.TryAdd(ServiceDescriptor.Singleton<ISqlConnectionFactory, SqlConnectionFactory>());
        services.TryAdd(ServiceDescriptor.Scoped<IUnitOfWork, UnitOfWork>());
        services.TryAdd(ServiceDescriptor.Scoped<IAccountsRepository, AccountsRepository>());
        services.TryAdd(ServiceDescriptor.Scoped<ITransactionsRepository, TransactionsRepository>());
        services.TryAdd(ServiceDescriptor.Scoped<ITransactionImportJobRepository, TransactionImportJobRepository>());
        return services;
    }
}