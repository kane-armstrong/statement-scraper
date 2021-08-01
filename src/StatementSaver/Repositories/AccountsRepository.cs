using System.Collections.Generic;
using Contracts;
using Dapper;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StatementSaver.Repositories
{
    public class AccountsRepository : IAccountsRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountsRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Account>> GetAccounts()
        {
            const string sql = @"
SELECT *
FROM [Staging].[Account]
";
            return await _unitOfWork.Connection.QueryAsync<Account>(sql, null, _unitOfWork.Transaction);
        }

        public async Task Save(Account entity, CancellationToken cancellationToken)
        {
            const string sql = @"
MERGE INTO [staging].[Account] AS [target]
USING 
(
    SELECT
        @Id                     AS [Id],
        @Identifier             AS [Identifier],
        @AccountType            AS [AccountType],
        @CardOrAccountNumber    AS [CardOrAccountNumber]
) AS [source]
ON (
    [target].[Identifier] = [source].Identifier
)
WHEN NOT MATCHED THEN
    INSERT
    (
        [Id],
        [Identifier],
        [AccountType],
        [CardOrAccountNumber]
    )
    VALUES 
    (
        [source].[Id],
        [source].[Identifier],
        [source].[AccountType],
        [source].[CardOrAccountNumber]
    )
WHEN MATCHED THEN
    UPDATE SET
        [target].[AccountType]              = [source].[AccountType],
        [target].[CardOrAccountNumber]      = [source].[CardOrAccountNumber]
OUTPUT $action AS MergeAction, inserted.Id;
;";

            var results = await _unitOfWork.Connection.QueryAsync<MergeResult>(sql, new
            {
                AccountType = (int)entity.AccountType,
                entity.Id,
                entity.Identifier,
                entity.CardOrAccountNumber
            }, _unitOfWork.Transaction);
            var result = results.Single();
            entity.Id = result.Id;
        }
    }
}
