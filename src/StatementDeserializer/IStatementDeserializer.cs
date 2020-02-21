using Contracts;

namespace StatementDeserializer
{
    public interface IStatementDeserializer
    {
        TransactionsSnapshot? DeserializeTdv(byte[] fileBytes);
        AccountType AccountType { get; }
    }
}