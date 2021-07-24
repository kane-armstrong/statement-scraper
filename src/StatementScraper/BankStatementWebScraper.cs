using Contracts;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using StatementScraper.Extensions;
using StatementScraper.Pages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatementScraper
{
    public class BankStatementWebScraper : IBankStatementWebScraper
    {
        private readonly BankStatementWebScraperOptions _options;

        public BankStatementWebScraper(IOptions<BankStatementWebScraperOptions> options)
        {
            _options = options.Value;
        }

        public async Task<IEnumerable<ScrapedAccount>> GetAccounts()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();
            await page.SetDownloadPath(_options.DownloadPath);
            var loginPage = new LoginPage(page);
            var balancesPage = await loginPage.Login(_options.UserName, _options.Password);
            var exportStatementsPage = await balancesPage.GoToExportStatementPage();
            return await exportStatementsPage.ListAccounts();
        }

        public async Task<DownloadResult> DownloadStatement(Account account, DateTimeOffset start, DateTimeOffset end)
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();
            await page.SetDownloadPath(_options.DownloadPath);
            var loginPage = new LoginPage(page);
            var balancesPage = await loginPage.Login(_options.UserName, _options.Password);
            var exportStatementsPage = await balancesPage.GoToExportStatementPage();
            return await exportStatementsPage.DownloadStatement(account, start, end, _options.ExportFormat);
        }
    }
}
