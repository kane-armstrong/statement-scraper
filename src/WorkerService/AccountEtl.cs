using Contracts;
using StatementSaver;
using StatementSaver.Repositories;
using StatementScraper;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerService;

public class AccountEtl
{
    private readonly IBankStatementWebScraper _bankStatementWebScraper;
    private readonly IAccountsRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;

    public AccountEtl(
        IBankStatementWebScraper bankStatementWebScraper,
        IAccountsRepository accounts,
        IUnitOfWork unitOfWork)
    {
        _bankStatementWebScraper = bankStatementWebScraper;
        _accounts = accounts;
        _unitOfWork = unitOfWork;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        await _unitOfWork.Start(cancellationToken);
        _unitOfWork.BeginTransaction();

        var scrapedAccounts = await _bankStatementWebScraper.GetAccounts();
        var savedAccounts = await _accounts.GetAccounts();

        foreach (var scrapedAccount in scrapedAccounts)
        {
            var matchedKnownAccount = savedAccounts.FirstOrDefault(
                x => x.Identifier.Equals(scrapedAccount.Identifier, StringComparison.InvariantCultureIgnoreCase));
            Account account;
            if (matchedKnownAccount != null)
            {
                matchedKnownAccount.Identifier = scrapedAccount.Identifier;
                account = matchedKnownAccount;
            }
            else
            {
                account = new Account
                {
                    Id = Guid.NewGuid(),
                    AccountType = scrapedAccount.AccountType,
                    Identifier = scrapedAccount.Identifier
                };
            }
            await _accounts.Save(account, cancellationToken);
        }

        _unitOfWork.Commit();
    }
}