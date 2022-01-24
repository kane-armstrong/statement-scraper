using Microsoft.Extensions.Logging;
using StatementSaver.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerService;

public class EtlRunner
{
    private readonly AccountEtl _accountEtl;
    private readonly TransactionEtl _transactionEtl;
    private readonly IAccountsRepository _accounts;
    private readonly ILogger<EtlRunner> _logger;

    public EtlRunner(
        AccountEtl accountEtl,
        TransactionEtl transactionEtl,
        IAccountsRepository accounts,
        ILogger<EtlRunner> logger)
    {
        _accountEtl = accountEtl;
        _transactionEtl = transactionEtl;
        _accounts = accounts;
        _logger = logger;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running account ETL");
        await _accountEtl.Run(cancellationToken);
        _logger.LogInformation("Account ETL ran successfully");

        var accounts = await _accounts.GetAccounts();
        foreach (var account in accounts)
        {
            _logger.LogInformation("Running transaction ETL for account: {CardOrAccountNumber}", account.CardOrAccountNumber);
            await _transactionEtl.Run(account, cancellationToken);
            _logger.LogInformation("Finished running transaction ETL for account: {CardOrAccountNumber}", account.CardOrAccountNumber);
        }
    }
}