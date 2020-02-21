using Contracts;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StatementSaver.Repositories
{
    public class StatementRunRepository : IStatementRunRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        public StatementRunRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Save(StatementRun entity, CancellationToken cancellationToken)
        {
            const string sql = @"
MERGE INTO [staging].[StatementRun] AS [target]
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
        [FromDate],
        [ToDate],
        [AccountId],
        [TransactionCount],
        [Status],
        [SourceFileName]
    )
    VALUES 
    (
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

            var results = await _unitOfWork.Connection.QueryAsync<MergeResult>(sql, entity, _unitOfWork.Transaction);
            var result = results.SingleOrDefault();
            if (result != null)
                entity.Id = result.Id;
        }

        public async Task<IEnumerable<StatementRun>> GetRuns(Account account, CancellationToken cancellationToken)
        {
            const string sql = @"
SELECT s.*
FROM [Staging].[StatementRun] AS s
WHERE s.AccountId = @AccountId
";

            return await _unitOfWork.Connection.QueryAsync<StatementRun>(sql, new
            {
                AccountId = account.Id
            }, _unitOfWork.Transaction);
        }
    }
}
