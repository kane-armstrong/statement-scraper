using Contracts;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StatementSaver.Repositories;

public interface IAccountsRepository
{
    // TODO cancellationtoken
    Task<IEnumerable<Account>> GetAccounts();
    Task Save(Account entity, CancellationToken cancellationToken);
}