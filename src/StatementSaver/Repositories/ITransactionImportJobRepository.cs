using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts;

namespace StatementSaver.Repositories;

public interface ITransactionImportJobRepository
{
    Task Save(TransactionImportJob entity, CancellationToken cancellationToken);
    Task<IEnumerable<TransactionImportJob>> ListJobs(Account account, CancellationToken cancellationToken);
}