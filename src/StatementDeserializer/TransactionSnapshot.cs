using Contracts;
using System.Collections.Generic;

namespace StatementDeserializer
{
    public class TransactionsSnapshot
    {
        public string CardOrAccountNumber { get; set; } = string.Empty;
        public IList<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
