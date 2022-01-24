using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace StatementSaver;

public interface IUnitOfWork
{
    Task Start(CancellationToken cancellationToken);
    IDbConnection? Connection { get; }
    IDbTransaction? Transaction { get; }
    void BeginTransaction();
    void Commit();
    void Rollback();
}