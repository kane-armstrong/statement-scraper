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

namespace WorkerService;

public class TransactionEtl
{
    private readonly ILogger<TransactionEtl> _logger;
    private readonly IBankStatementWebScraper _bankStatementWebScraper;
    private readonly IStatementFactory _statementFactory;
    private readonly ITransactionImportJobRepository _transactionImportJobs;
    private readonly IAccountsRepository _accounts;
    private readonly ITransactionsRepository _transactions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TransactionEtlOptions _options;

    private readonly string _unprocessedPath;
    private readonly string _processedPath;
    private readonly bool _moveProcessedStatements;

    public TransactionEtl(
        ILogger<TransactionEtl> logger,
        IBankStatementWebScraper bankStatementWebScraper,
        IStatementFactory statementFactory,
        ITransactionImportJobRepository transactionImportJobs,
        IAccountsRepository accounts,
        ITransactionsRepository transactions,
        IUnitOfWork unitOfWork,
        IOptions<TransactionEtlOptions> options)
    {
        _logger = logger;
        _bankStatementWebScraper = bankStatementWebScraper;
        _statementFactory = statementFactory;
        _transactionImportJobs = transactionImportJobs;
        _accounts = accounts;
        _transactions = transactions;
        _unitOfWork = unitOfWork;
        _options = options.Value;

        _unprocessedPath = options.Value.UnprocessedStatementDirectory
                           ?? throw new ArgumentNullException(
                               nameof(options.Value.UnprocessedStatementDirectory),
                               "Unprocessed statement directory is required");

        if (!Directory.Exists(_unprocessedPath))
            Directory.CreateDirectory(_unprocessedPath);

        _processedPath = options.Value.ProcessedStatementDirectory
                         ?? throw new ArgumentNullException(
                             nameof(options.Value.ProcessedStatementDirectory),
                             "Processed statement directory is required");

        if (!Directory.Exists(_processedPath))
            Directory.CreateDirectory(_processedPath);

        _moveProcessedStatements = options.Value.MoveProcessedStatements;
    }

    public async Task Run(Account account, CancellationToken cancellationToken)
    {
        await _unitOfWork.Start(cancellationToken);

        CleanWorkingDirectory();

        var job = await CreateTransactionImportJob(account, cancellationToken);

        try
        {
            var statementFilePath = await DownloadStatement(account, job, cancellationToken);
            if (statementFilePath == null)
            {
                return;
            }

            var statement = await LoadStatement(account, cancellationToken, statementFilePath);

            _logger.LogInformation("Processing file '{file}'", statementFilePath);

            if (statement == null)
                throw new Exception("Failed to load the statement. Found the file, but did not recognize its content.");

            _unitOfWork.BeginTransaction();

            job.SourceFileName = Path.GetFileName(statementFilePath);
            job.TransactionCount = statement.Transactions.Count;
            account.CardOrAccountNumber = statement.CardOrAccountNumber;

            _logger.LogDebug("Processing statement. Contains {count} transactions", job.TransactionCount);

            foreach (var importedTransaction in statement.Transactions)
            {
                var transaction = new Transaction
                {
                    AccountId = account.Id,
                    Amount = importedTransaction.Amount,
                    Id = Guid.NewGuid(),
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
            await _accounts.Save(account, cancellationToken);

            if (_moveProcessedStatements)
            {
                var newPath = $"{_processedPath}\\{job.SourceFileName}";
                _logger.LogDebug("Moving processed file {file} to path {path}", statementFilePath, newPath);
                File.Move(statementFilePath, newPath);
            }

            _logger.LogInformation("Finished loading transactions for account: {accountId} - {accountIdentifier}", account.Id,
                account.Identifier);

            _unitOfWork.Commit();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load statements for account {account}: {message}", account.Identifier, e.Message);
            _unitOfWork.Rollback();

            _unitOfWork.BeginTransaction();
            job.Status = "Failed: exception";
            await _transactionImportJobs.Save(job, cancellationToken);
            _unitOfWork.Commit();
        }
    }

    private async Task<Statement?> LoadStatement(Account account, CancellationToken cancellationToken, string statementFilePath)
    {
        var bytes = await File.ReadAllBytesAsync(statementFilePath, cancellationToken);
        var statement = _statementFactory.Create(bytes)[account.AccountType];
        return statement;
    }

    private void CleanWorkingDirectory()
    {
        var unprocessedFiles = Directory.GetFiles(_unprocessedPath);
        if (!unprocessedFiles.Any()) return;
        foreach (var existingFile in unprocessedFiles)
        {
            File.Delete(existingFile);
        }
    }

    private async Task<TransactionImportJob> CreateTransactionImportJob(Account account, CancellationToken cancellationToken)
    {
        _unitOfWork.BeginTransaction();

        var jobs = await _transactionImportJobs.ListJobs(account, cancellationToken);
        
        var latestJob = jobs.Where(x => x.TransactionCount > 0 && x.AccountId == account.Id)
            .OrderByDescending(x => x.ToDate).FirstOrDefault();

        const int yearsToCapture = 3;
        var fromDate = latestJob?.ToDate ?? DateTimeOffset.Now.AddYears(-yearsToCapture);

        var job = new TransactionImportJob
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            FromDate = fromDate,
            ToDate = DateTimeOffset.Now,
            TransactionCount = 0,
            Status = "Pending"
        };

        await _transactionImportJobs.Save(job, cancellationToken);
        _unitOfWork.Commit();
        return job;
    }

    private async Task<string> DownloadStatement(Account account, TransactionImportJob job, CancellationToken cancellationToken)
    {
        var downloadResult = await _bankStatementWebScraper.DownloadStatement(account, job.FromDate, job.ToDate);
        job.Status = downloadResult.StatusMessage;

        if (!downloadResult.Successful)
        {
            _logger.LogWarning("Failed to download statement for {account} for the period {start} to {end}. Reason: {reason}",
                job.AccountId, job.FromDate.ToString("yyyy-MM-dd"), job.ToDate.ToString("yyyy-MM-dd"),
                downloadResult.StatusMessage);
            _unitOfWork.BeginTransaction();
            await _transactionImportJobs.Save(job, cancellationToken);
            _unitOfWork.Commit();

            var unexpectedFiles = Directory.GetFiles(_options.UnprocessedStatementDirectory);
            if (unexpectedFiles.Any())
            {
                _logger.LogError("Found unprocessed statements where none were expected.");
                throw new InvalidOperationException($"Scraper reported no downloaded files, but files were found in the directory. Path: '{_options.UnprocessedStatementDirectory}'");
            }

            return null;
        }

        var files = Directory.GetFiles(_options.UnprocessedStatementDirectory);
        return files.Single();
    }
}