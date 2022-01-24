using Contracts;
using System.Collections.Generic;
using System.Linq;

namespace StatementDeserializer;

public class StatementFactory : IStatementFactory
{
    private readonly IEnumerable<IStatementDeserializer> _deserializers;

    public StatementFactory(IEnumerable<IStatementDeserializer> deserializers) =>
        _deserializers = deserializers;

    public Dictionary<AccountType, Statement?> Create(byte[] bytes) =>
        _deserializers.ToDictionary(c => c.AccountType, c => c.DeserializeTdv(bytes));
}