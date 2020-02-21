using Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatementScraper
{
    public interface IBankStatementWebScraper
    {
        Task<IEnumerable<ScrapedAccount>> GetAccounts();
        Task<DownloadResult> DownloadStatement(Account account, DateTimeOffset start, DateTimeOffset end);
    }
}
