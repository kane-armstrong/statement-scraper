using Contracts;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StatementSaver.Repositories;

public class TransactionImportJobRepository : ITransactionImportJobRepository
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionImportJobRepository(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Save(TransactionImportJob entity, CancellationToken cancellationToken)
    {
        const string sql = @"
MERGE INTO [staging].[TransactionImportJob] AS [target]
USING 
(
    SELECT
        @Id                 AS [Id],
        @FromDate           AS [FromDate],
        @ToDate             AS [ToDate],
        @AccountId          AS [AccountId],
        @TransactionCount   AS [TransactionCount],
        @Status             AS [Status],
        @SourceFileName     AS [SourceFileName]
) AS [source]
ON (
    [target].[FromDate] = [source].FromDate AND [target].[ToDate] = [source].ToDate AND [target].AccountId = [source].AccountId
)
WHEN NOT MATCHED THEN
    INSERT
    (
        [Id],
        [FromDate],
        [ToDate],
        [AccountId],
        [TransactionCount],
        [Status],
        [SourceFileName]
    )
    VALUES 
    (
        [source].[Id],
        [source].[FromDate],
        [source].[ToDate],
        [source].[AccountId],
        [source].[TransactionCount],
        [source].[Status],
        [source].[SourceFileName]
    )
WHEN MATCHED THEN
    UPDATE SET
        [target].[FromDate]         =    [source].[FromDate],
        [target].[ToDate]           =    [source].[ToDate],
        [target].[AccountId]        =    [source].[AccountId],
        [target].[TransactionCount] =    [source].[TransactionCount],
        [target].[Status]           =    [source].[Status],
        [target].[SourceFileName]   =    [source].[SourceFileName]
OUTPUT $action AS MergeAction, inserted.Id
;";

        var command = new CommandDefinition(sql, entity, _unitOfWork.Transaction, cancellationToken: cancellationToken);
        var results = await _unitOfWork.Connection.QueryAsync<MergeResult>(command);
        var result = results.SingleOrDefault();
        if (result != null)
            entity.Id = result.Id;
    }

    public async Task<IEnumerable<TransactionImportJob>> ListJobs(Account account, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT s.*
FROM [Staging].[TransactionImportJob] AS s
WHERE s.AccountId = @AccountId
";

        var command = new CommandDefinition(sql, new
        {
            AccountId = account.Id
        }, _unitOfWork.Transaction, cancellationToken: cancellationToken);

        return await _unitOfWork.Connection.QueryAsync<TransactionImportJob>(command);
    }
}