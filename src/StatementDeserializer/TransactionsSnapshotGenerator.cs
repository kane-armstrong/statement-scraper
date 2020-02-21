using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatementDeserializer
{
    public class TransactionsSnapshotGenerator : ITransactionsSnapshotGenerator
    {
        private readonly IEnumerable<IStatementDeserializer> _deserializers;

        public TransactionsSnapshotGenerator(IEnumerable<IStatementDeserializer> deserializers)
        {
            _deserializers = deserializers ?? throw new ArgumentNullException(nameof(deserializers));
        }

        public Dictionary<AccountType, TransactionsSnapshot?> Generate(byte[] bytes) =>
            _deserializers.ToDictionary(c => c.AccountType, c => c.DeserializeTdv(bytes));
    }
}
