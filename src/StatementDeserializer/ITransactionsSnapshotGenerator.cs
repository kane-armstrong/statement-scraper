using Contracts;
using System.Collections.Generic;

namespace StatementDeserializer
{
    public interface ITransactionsSnapshotGenerator
    {
        Dictionary<AccountType, TransactionsSnapshot?> Generate(byte[] bytes);
    }
}