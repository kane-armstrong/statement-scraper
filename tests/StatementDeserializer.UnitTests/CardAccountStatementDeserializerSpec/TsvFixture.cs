using StatementDeserializer.UnitTests.Resources.Content.CardAccounts;

namespace StatementDeserializer.UnitTests.CardAccountStatementDeserializerSpec
{
    public class TsvFixture
    {
        public readonly Statement Statement;

        public TsvFixture()
        {
            var sut = new CardAccountStatementDeserializer();
            Statement = sut.DeserializeTdv(SampleCardAccountReports.BasicTdv);
        }
    }
}