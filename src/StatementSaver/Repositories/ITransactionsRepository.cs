using System.Threading;
using System.Threading.Tasks;
using Contracts;

namespace StatementSaver.Repositories
{
    public interface ITransactionsRepository
    {
        Task Save(Transaction entity, CancellationToken cancellationToken);
    }
}