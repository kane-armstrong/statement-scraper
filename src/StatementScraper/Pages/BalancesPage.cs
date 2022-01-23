using PuppeteerSharp;
using System;
using System.Threading.Tasks;

namespace StatementScraper.Pages;

internal class BalancesPage
{
    private readonly Page _page;

    private const string StatementsPageLink = "/fnc/1/goto/statements";

    public BalancesPage(Page page)
    {
        _page = page;
    }

    public async Task<ExportStatementPage> GoToExportStatementPage()
    {
        const int timeoutMs = 5000;
        var uri = new Uri(_page.Url);
        var exportStatementsPage = $"{uri.Scheme}://{uri.Host}{StatementsPageLink}";
        await _page.GoToAsync(exportStatementsPage, timeoutMs);
        return new ExportStatementPage(_page);
    }
}