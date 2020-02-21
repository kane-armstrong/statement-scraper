using StatementDeserializer.UnitTests.Resources.Content.DepositAccounts;

namespace StatementDeserializer.UnitTests.DepositAccountStatementDeserializerSpec
{
    public class TsvFixture
    {
        public readonly TransactionsSnapshot Snapshot;

        public TsvFixture()
        {
            var sut = new DepositAccountStatementDeserializer();
            Snapshot = sut.DeserializeTdv(SampleDepositAccountReports.BasicTdv);
        }
    }
}