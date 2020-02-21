using Pedantic.IO;

namespace StatementDeserializer.UnitTests.Resources.Content.DepositAccounts
{
    public class SampleDepositAccountReports
    {
        public static byte[] BasicTdv => LoadContent("TdvDepositAccountSample.tdv");
        public static byte[] TdvAccountMalformedBank => LoadContent("TdvDepositAccountSample_bad-bank.tdv");
        public static byte[] TdvAccountMalformedBranch => LoadContent("TdvDepositAccountSample_bad-branch.tdv");
        public static byte[] TdvTransactionMalformedDate => LoadContent("TdvDepositAccountSample_bad-transaction-date.tdv");
        public static byte[] TdvTransactionMalformedAmount => LoadContent("TdvDepositAccountSample_bad-transaction-amount.tdv");

        private static byte[] LoadContent(string fileName)
        {
            var type = typeof(SampleDepositAccountReports);
            return EmbeddedResource.ReadAllBytes(type.Assembly, $"{type.Namespace}.{fileName}");
        }
    }
}