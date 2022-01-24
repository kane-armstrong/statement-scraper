using Contracts;
using Dapper;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StatementSaver.Repositories;

public class TransactionsRepository : ITransactionsRepository
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionsRepository(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Save(Transaction entity, CancellationToken cancellationToken)
    {
        const string sql = @"
MERGE INTO [Staging].[Transaction] AS [target]
USING 
(
    SELECT
        @Id                 AS [Id],
        @AccountId          AS [AccountId],
        @TransactionDate    AS [TransactionDate],
        @UniqueId           AS [UniqueId],
        @TransactionType    AS [TransactionType],
        @Payee              AS [Payee],
        @CardNumber         AS [CardNumber],
        @Description        AS [Description],
        @Amount             AS [Amount]
) AS [source]
ON 
(
    [source].[UniqueId] = [target].UniqueId AND [source].[AccountId] = [target].AccountId
)
WHEN NOT MATCHED THEN
    INSERT
    (
        [Id],
        [AccountId],
        [TransactionDate],
        [UniqueId],
        [TransactionType],
        [Payee],
        [CardNumber],
        [Description],
        [Amount]
    )
    VALUES 
    (
        [source].[Id],
        [source].[AccountId],
        [source].[TransactionDate],
        [source].[UniqueId],
        [source].[TransactionType],
        [source].[Payee],
        [source].[CardNumber],
        [source].[Description],
        [source].[Amount]
    )
WHEN MATCHED THEN
    UPDATE SET
        [target].[AccountId]         = [source].[AccountId],
        [target].[TransactionDate]   = [source].[TransactionDate],
        [target].[UniqueId]          = [source].[UniqueId],
        [target].[TransactionType]   = [source].[TransactionType],
        [target].[Payee]             = [source].[Payee],
        [target].[CardNumber]        = [source].[CardNumber],
        [target].[Description]       = [source].[Description],
        [target].[Amount]            = [source].[Amount]
OUTPUT $action AS MergeAction, inserted.Id;
;";

        var command = new CommandDefinition(sql, entity, _unitOfWork.Transaction, cancellationToken: cancellationToken);
        var results = await _unitOfWork.Connection.QueryAsync<MergeResult>(command);
        var result = results.Single();
        entity.Id = result.Id;
    }
}