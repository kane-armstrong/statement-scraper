using Contracts;

namespace StatementDeserializer
{
    public interface IStatementDeserializer
    {
        Statement? DeserializeTdv(byte[] fileBytes);
        AccountType AccountType { get; }
    }
}