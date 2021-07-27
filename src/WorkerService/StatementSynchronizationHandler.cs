﻿using Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StatementDeserializer;
using StatementSaver;
using StatementSaver.Repositories;
using StatementScraper;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerService
{
    public class StatementSynchronizationHandler
    {
        private readonly ILogger<StatementSynchronizationHandler> _logger;
        private readonly IBankStatementWebScraper _bankStatementWebScraper;
        private readonly IStatementFactory _snapshotGenerator;
        private readonly ITransactionImportJobRepository _transactionImportJobs;
        private readonly IAccountsRepository _accounts;
        private readonly ITransactionsRepository _transactions;
        private readonly IUnitOfWork _unitOfWork;
        private readonly StatementSynchronizationHandlerOptions _options;

        private readonly string _unprocessedPath;
        private readonly string _processedPath;
        private readonly bool _moveProcessedStatements;

        public StatementSynchronizationHandler(
            ILogger<StatementSynchronizationHandler> logger,
            IBankStatementWebScraper bankStatementWebScraper,
            IStatementFactory snapshotGenerator,
            ITransactionImportJobRepository transactionImportJobs,
            IAccountsRepository accounts,
            ITransactionsRepository transactions,
            IUnitOfWork unitOfWork,
            IOptions<StatementSynchronizationHandlerOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bankStatementWebScraper = bankStatementWebScraper ?? throw new ArgumentNullException(nameof(bankStatementWebScraper));
            _snapshotGenerator = snapshotGenerator ?? throw new ArgumentNullException(nameof(snapshotGenerator));
            _transactionImportJobs = transactionImportJobs ?? throw new ArgumentNullException(nameof(transactionImportJobs));
            _accounts = accounts ?? throw new ArgumentNullException(nameof(accounts));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));

            _unprocessedPath = options.Value.UnprocessedStatementDirectory ?? throw new ArgumentNullException(nameof(options.Value.UnprocessedStatementDirectory));
            if (!Directory.Exists(_unprocessedPath))
                Directory.CreateDirectory(_unprocessedPath);

            _processedPath = options.Value.ProcessedStatementDirectory ?? throw new ArgumentNullException(nameof(options.Value.ProcessedStatementDirectory));
            if (!Directory.Exists(_processedPath))
                Directory.CreateDirectory(_processedPath);

            _moveProcessedStatements = options.Value.MoveProcessedStatements;
        }

        public async Task DownloadBankTransactions(CancellationToken cancellationToken)
        {
            await _unitOfWork.Start(cancellationToken);

            CleanWorkingDirectory(_unprocessedPath);

            var accounts = await _accounts.GetAccounts();
            foreach (var account in accounts)
            {
                _unitOfWork.BeginTransaction();
                var currentRun = await CreateStatementRun(account, cancellationToken);
                _unitOfWork.Commit();

                try
                {
                    _unitOfWork.BeginTransaction();
                    await SynchronizeAccountTransactions(account, currentRun, cancellationToken);
                    _unitOfWork.Commit();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to load statements for account {account}: {message}", account.Identifier, e.Message);
                    _unitOfWork.Rollback();

                    _unitOfWork.BeginTransaction();
                    currentRun.Status = "Failed: exception";
                    await _transactionImportJobs.Save(currentRun, cancellationToken);
                    _unitOfWork.Commit();
                }
            }
        }

        private static void CleanWorkingDirectory(string path)
        {
            var existingFiles = Directory.GetFiles(path);
            if (existingFiles.Any())
            {
                foreach (var existingFile in existingFiles)
                {
                    File.Delete(existingFile);
                }
            }
        }

        private async Task<TransactionImportJob> CreateStatementRun(Account account, CancellationToken cancellationToken)
        {
            var runs = await _transactionImportJobs.ListJobs(account, cancellationToken);
            var latestRun = runs.Where(x => x.TransactionCount > 0 && x.AccountId == account.Id)
                .OrderByDescending(x => x.ToDate).FirstOrDefault();
            var fromDate = latestRun?.ToDate ?? DateTimeOffset.Now.AddYears(-3);

            var currentRun = new TransactionImportJob
            {
                AccountId = account.Id,
                FromDate = fromDate,
                ToDate = DateTimeOffset.Now,
                TransactionCount = 0,
                Status = "Pending"
            };
            await _transactionImportJobs.Save(currentRun, cancellationToken);
            return currentRun;
        }

        private async Task SynchronizeAccountTransactions(Account account, TransactionImportJob currentRun, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Loading transactions for account: {accountId} - {accountIdentifier}", account.Id, account.Identifier);

            var exportedStatement = await _bankStatementWebScraper.DownloadStatement(account, currentRun.FromDate, currentRun.ToDate);
            currentRun.Status = exportedStatement.StatusMessage;

            if (!exportedStatement.Successful)
            {
                _logger.LogWarning("Failed to download statement for {account} for the period {start} to {end}. Reason: {reason}",
                    currentRun.AccountId, currentRun.FromDate.ToString("yyyy-MM-dd"), currentRun.ToDate.ToString("yyyy-MM-dd"),
                    exportedStatement.StatusMessage);
                await _transactionImportJobs.Save(currentRun, cancellationToken);
                EnsureNoUnprocessedStatements();
                return;
            }

            var files = EnsureUnprocessedStatements();

            var file = files[0];
            var filename = Path.GetFileName(file);
            _logger.LogInformation("Processing file '{file}'", file);

            var snapshot = await ReadStatement(account, file);

            currentRun.SourceFileName = filename;
            currentRun.TransactionCount = snapshot.Transactions.Count;
            account.CardOrAccountNumber = snapshot.CardOrAccountNumber;

            await LoadStatementIntoDatabase(account, currentRun, snapshot, cancellationToken);

            await _accounts.Save(account, cancellationToken);

            if (_moveProcessedStatements)
            {
                var newPath = $"{_processedPath}\\{filename}";
                _logger.LogDebug("Moving processed file {file} to path {path}", file, newPath);
                File.Move(file, newPath);
            }

            _logger.LogInformation("Finished loading transactions for account: {accountId} - {accountIdentifier}", account.Id, account.Identifier);
        }

        private async Task<Statement> ReadStatement(Account account, string file)
        {
            var bytes = await File.ReadAllBytesAsync(file);
            var snapshot = _snapshotGenerator.Create(bytes)[account.AccountType];

            if (snapshot == null)
                throw new Exception("Failed to deserialize the statement. Found and loaded the file, but did not recognize its content.");
            return snapshot;
        }

        private string[] EnsureUnprocessedStatements()
        {
            var files = Directory.GetFiles(_options.UnprocessedStatementDirectory);
            if (!files.Any())
            {
                throw new InvalidOperationException($"No statements were found in directory '{_options.UnprocessedStatementDirectory}'");
            }

            return files;
        }

        private void EnsureNoUnprocessedStatements()
        {
            var unexpectedFiles = Directory.GetFiles(_options.UnprocessedStatementDirectory);
            if (unexpectedFiles.Any())
            {
                _logger.LogError("Found unprocessed statements where none were expected.");
                throw new InvalidOperationException($"Scraper reported no downloaded files, but files were found in the directory. Path: '{_options.UnprocessedStatementDirectory}'");
            }
        }

        private async Task LoadStatementIntoDatabase(Account account, TransactionImportJob job, Statement statement, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Processing statement. Contains {count} transactions", job.TransactionCount);

            foreach (var importedTransaction in statement.Transactions)
            {
                var transaction = new Transaction
                {
                    AccountId = account.Id,
                    Amount = importedTransaction.Amount,
                    Id = 0,
                    CardNumber = importedTransaction.CardNumber,
                    Description = importedTransaction.Description,
                    Payee = importedTransaction.Payee,
                    TransactionDate = importedTransaction.TransactionDate,
                    TransactionType = importedTransaction.TransactionType,
                    UniqueId = importedTransaction.UniqueId
                };
                await _transactions.Save(transaction, cancellationToken);
            }

            await _transactionImportJobs.Save(job, cancellationToken);
        }
    }
}
