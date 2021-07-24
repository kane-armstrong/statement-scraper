using PuppeteerSharp;
using StatementScraper.Extensions;
using System;
using System.Threading.Tasks;

namespace StatementScraper.Pages
{
    internal class BalancesPage
    {
        private readonly Page _page;

        public BalancesPage(Page page)
        {
            _page = page;
        }

        public async Task<ExportStatementPage> GoToExportStatementPage()
        {
            var statementsLink = await _page.GetElementOrDefault(ElementSelectors.StatementsPageLink);
            if (statementsLink == null)
            {
                throw new Exception($"Failed to browse to statement export page: select '{ElementSelectors.StatementsPageLink}' was not found on the page.");
            }

            await statementsLink.ClickAsync();
            await _page.WaitForNavigationAsync();

            return new ExportStatementPage(_page);
        }
    }
}
