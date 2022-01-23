using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace StatementSaver;

public class UnitOfWork : IDisposable, IUnitOfWork
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public IDbConnection Connection { get; private set; }
    public IDbTransaction Transaction { get; private set; }

    public UnitOfWork(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory ?? throw new ArgumentNullException(nameof(sqlConnectionFactory));
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        if (Connection != null && Connection.State == ConnectionState.Open)
            return;

        Connection = await _sqlConnectionFactory.GetOpenConnection(cancellationToken);
    }

    public void BeginTransaction()
    {
        Transaction = Connection.BeginTransaction();
    }

    public void Commit()
    {
        if (Transaction == null)
            throw new InvalidOperationException("There is no transaction to commit");
        Transaction.Commit();
        Transaction.Dispose();
        Transaction = null;
    }

    public void Rollback()
    {
        if (Transaction == null)
            throw new InvalidOperationException("There is no transaction to rollback");
        Transaction.Rollback();
        Transaction.Dispose();
        Transaction = null;
    }

    public void Dispose()
    {
        Transaction?.Dispose();
        Connection?.Dispose();
    }
}