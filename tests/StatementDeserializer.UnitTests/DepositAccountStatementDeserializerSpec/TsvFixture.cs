using StatementDeserializer.UnitTests.Resources.Content.DepositAccounts;

namespace StatementDeserializer.UnitTests.DepositAccountStatementDeserializerSpec
{
    public class TsvFixture
    {
        public readonly Statement Statement;

        public TsvFixture()
        {
            var sut = new DepositAccountStatementDeserializer();
            Statement = sut.DeserializeTdv(SampleDepositAccountReports.BasicTdv);
        }
    }
}