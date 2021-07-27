using Contracts;
using System.Collections.Generic;

namespace StatementDeserializer
{
    public interface IStatementFactory
    {
        Dictionary<AccountType, Statement?> Create(byte[] bytes);
    }
}