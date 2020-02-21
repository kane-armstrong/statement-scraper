using StatementDeserializer.UnitTests.Resources.Content.CardAccounts;

namespace StatementDeserializer.UnitTests.CardAccountStatementDeserializerSpec
{
    public class TsvFixture
    {
        public readonly TransactionsSnapshot Snapshot;

        public TsvFixture()
        {
            var sut = new CardAccountStatementDeserializer();
            Snapshot = sut.DeserializeTdv(SampleCardAccountReports.BasicTdv);
        }
    }
}