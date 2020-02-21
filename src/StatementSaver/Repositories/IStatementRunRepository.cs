using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts;

namespace StatementSaver.Repositories
{
    public interface IStatementRunRepository
    {
        Task Save(StatementRun entity, CancellationToken cancellationToken);
        Task<IEnumerable<StatementRun>> GetRuns(Account account, CancellationToken cancellationToken);
    }
}