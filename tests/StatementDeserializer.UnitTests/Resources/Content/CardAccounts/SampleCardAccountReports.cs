using Pedantic.IO;

namespace StatementDeserializer.UnitTests.Resources.Content.CardAccounts
{
    public class SampleCardAccountReports
    {
        public static byte[] BasicTdv => LoadContent("TdvCardAccountSample.tdv");

        private static byte[] LoadContent(string fileName)
        {
            var type = typeof(SampleCardAccountReports);
            return EmbeddedResource.ReadAllBytes(type.Assembly, $"{type.Namespace}.{fileName}");
        }
    }
}