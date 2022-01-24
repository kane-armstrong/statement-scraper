using Contracts;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StatementSaver.Repositories;

public interface IAccountsRepository
{
    Task<IEnumerable<Account>> GetAccounts(CancellationToken cancellationToken);
    Task Save(Account entity, CancellationToken cancellationToken);
}