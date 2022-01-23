using Contracts;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using StatementScraper.Extensions;
using StatementScraper.Pages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatementScraper;

public class BankStatementWebScraper : IBankStatementWebScraper, IAsyncDisposable, IDisposable
{
    private readonly BankStatementWebScraperOptions _options;
    private Browser? _browser;
    private Page? _page;
    private ExportStatementPage? _exportStatementsPage;

    public BankStatementWebScraper(IOptions<BankStatementWebScraperOptions> options)
    {
        _options = options.Value;
    }

    private async Task<ExportStatementPage> LoadExportStatementsPage()
    {
        await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
        _browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        _page = await _browser.NewPageAsync();
        await _page.SetDownloadPath(_options.DownloadPath);
        var loginPage = new LoginPage(_page);
        var balancesPage = await loginPage.Login(_options.UserName, _options.Password);
        return await balancesPage.GoToExportStatementPage();
    }

    public async Task<IEnumerable<ScrapedAccount>> GetAccounts()
    {
        if (_exportStatementsPage == null)
        {
            _exportStatementsPage = await LoadExportStatementsPage();
        }
        return await _exportStatementsPage.ListAccounts();
    }

    public async Task<DownloadResult> DownloadStatement(Account account, DateTimeOffset start, DateTimeOffset end)
    {
        if (_exportStatementsPage == null)
        {
            _exportStatementsPage = await LoadExportStatementsPage();
        }

        return await _exportStatementsPage.DownloadStatement(account, start, end, _options.ExportFormat);
    }

    public async ValueTask DisposeAsync()
    {
        if (_page == null) return;
        await _page.DisposeAsync();
        if (_browser == null) return;
        await _browser.DisposeAsync();
    }

    public void Dispose()
    {
        _browser?.Dispose();
        _page?.Dispose();
    }
}