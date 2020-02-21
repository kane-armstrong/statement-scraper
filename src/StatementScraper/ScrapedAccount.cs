using Contracts;

namespace StatementScraper
{
    public class ScrapedAccount
    {
        public AccountType AccountType { get; }
        public string Identifier { get; }

        internal ScrapedAccount(AccountType accountType, string identifier)
        {
            AccountType = accountType;
            Identifier = identifier;
        }
    }
}
