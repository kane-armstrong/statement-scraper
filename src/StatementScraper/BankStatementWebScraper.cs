#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

using Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using StatementScraper.Extensions;
using StatementScraper.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace StatementScraper
{
    public class BankStatementWebScraper : IBankStatementWebScraper, IAsyncDisposable, IDisposable
    {
        private readonly Guid _diagnosticsSessionId = Guid.NewGuid();
        private readonly BankStatementWebScraperOptions _options;
        private readonly ILogger<BankStatementWebScraper> _logger;
        private readonly bool _takeScreenshots;
        private readonly string _selectedExportFormat;
        private readonly string? _screenshotOutputPath;

        private LoginPage _loginPage;

        private readonly string _downloadPath;

        private Browser? _browser;
        private Page? _page;

        private bool Initialized => _browser != null;
        private bool _loggedIn;

        private string? _statementPageUrl;

        public BankStatementWebScraper(IOptions<BankStatementWebScraperOptions> options, ILogger<BankStatementWebScraper> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(logger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Guard.AgainstNullOrEmpty(options.Value.UserName, nameof(BankStatementWebScraperOptions.UserName));
            Guard.AgainstNullOrEmpty(options.Value.Password, nameof(BankStatementWebScraperOptions.Password));

            if (options.Value.CollectScreenShotDiagnostics)
            {
                Guard.AgainstNullOrEmpty(options.Value.UserName, nameof(BankStatementWebScraperOptions.ScreenshotDiagnosticsPath));
                _takeScreenshots = true;
                _screenshotOutputPath = options.Value.ScreenshotDiagnosticsPath;
                if (!Directory.Exists(_screenshotOutputPath))
                    Directory.CreateDirectory(_screenshotOutputPath);
            }

            Guard.AgainstNullOrEmpty(options.Value.ExportFormat, nameof(BankStatementWebScraperOptions.ExportFormat));
            if (!ExportFormats.List().Contains(options.Value.ExportFormat))
                throw new ArgumentOutOfRangeException(nameof(options.Value.ExportFormat), $"Export format invalid. Valid options: {string.Join(", ", ExportFormats.List())}");
            _selectedExportFormat = options.Value.ExportFormat;

            Guard.AgainstNullOrEmpty(options.Value.DownloadPath, nameof(BankStatementWebScraperOptions.DownloadPath));
            _downloadPath = options.Value.DownloadPath;
            if (!Directory.Exists(_downloadPath))
                Directory.CreateDirectory(_downloadPath);
        }

        private async Task Initialize()
        {
            if (Initialized)
                throw new InvalidOperationException("Puppeteer is already initialized");

            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });

            _page = await _browser.NewPageAsync();
            _page.Client.MessageReceived += (sender, args) =>
            {
                _logger.LogTrace($"Message received from Chromium client: {args.MessageData}");
            };
            _page.Client.Disconnected += (sender, args) =>
            {
                _logger.LogTrace("Chromium DevTools client disconnected");
            };

            _logger.LogDebug("Download path set to {downloadPath}", _downloadPath);

            await _page.SetDownloadPath(_downloadPath);

            _loginPage = new LoginPage(_page);
        }

        private async Task NavigateToStatementExportPage()
        {
            var statementsLink = await _page.GetElementOrDefault(ElementSelectors.StatementsPageLink);
            if (statementsLink == null)
            {
                throw new Exception($"Failed to browse to statement export page: select '{ElementSelectors.StatementsPageLink}' was not found on the page.");
            }

            await statementsLink.ClickAsync();
            await _page.WaitForNavigationAsync();

            await TakeScreenshotIfEnabled("Statement export page after navigation");

            _statementPageUrl = _page.Url;
        }

        private async Task EnsureStatementPage()
        {
            if (_statementPageUrl != null)
            {
                if (_page.Url.Contains(_statementPageUrl))
                    return;

                await _page.GoToAsync(_statementPageUrl);

                // Session may have timed out, requiring login again
                var loginButton = await _page.GetElementOrDefault(ElementSelectors.LoginButton);
                if (loginButton == null)
                    return;

                _loggedIn = false;
                await _loginPage.BrowseTo();
                await _loginPage.Login(_options.UserName, _options.Password);

                return;
            }

            if (!Initialized)
                await Initialize();
            if (!_loggedIn)
            {
                await _loginPage.BrowseTo();
                await _loginPage.Login(_options.UserName, _options.Password);
            }

            await NavigateToStatementExportPage();
        }

        public async Task<IEnumerable<ScrapedAccount>> GetAccounts()
        {
            await EnsureStatementPage();

            var options = (await _page.GetSelectListOptions(ElementSelectors.AccountsSelectList)).ToList();

            const string depositAccount = "DepositAccount";
            const string cardAccount = "CardAccount";

            var items = new List<ScrapedAccount>();
            foreach (var option in options)
            {
                AccountType accountType;
                if (option.Contains(depositAccount))
                {
                    accountType = AccountType.DepositAccount;
                }
                else if (option.Contains(cardAccount))
                {
                    accountType = AccountType.CardAccount;
                }
                else
                {
                    throw new Exception($"Unrecognized scrapedAccount type: {option}");
                }

                items.Add(new ScrapedAccount(accountType, option));
            }

            return items;
        }

        public async Task<DownloadResult> DownloadStatement(Account account, DateTimeOffset start, DateTimeOffset end)
        {
            await EnsureStatementPage();

            _logger.LogInformation("Exporting report for account {account} in format {exportFormat} for period {start} to {end}",
                account.Identifier, _selectedExportFormat, start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));

            await _page.TypeInSelectedElement(ElementSelectors.FromDateInputDay, start.Day.ToString());
            await _page.TypeInSelectedElement(ElementSelectors.FromDateInputMonth, start.Month.ToString());
            await _page.TypeInSelectedElement(ElementSelectors.FromDateInputYear, start.Year.ToString());

            await _page.TypeInSelectedElement(ElementSelectors.ToDateInputDay, end.Day.ToString());
            await _page.TypeInSelectedElement(ElementSelectors.ToDateInputMonth, end.Month.ToString());
            await _page.TypeInSelectedElement(ElementSelectors.ToDateInputYear, end.Year.ToString());

            var exportButtonDiv = await _page.GetElement(ElementSelectors.ExportButtonContainer);

            await _page.SelectAsync(ElementSelectors.AccountsSelectList, account.Identifier);
            await _page.SelectAsync(ElementSelectors.ExportFormatSelectList, _selectedExportFormat);

            try
            {
                await exportButtonDiv.ClickAsync();

                // The site automatically reloads the page if it encounters any errors when attempting to generate the statement. I think they're using
                // ASP.NET and something along the lines of (return View(ModelState)) with a keyless entry added indicating there are no transactions for the specified
                // date range. We need to await these cases to avoid exceptions, but not await successful attempts to avoid timeout exceptions (as successful attempts
                // won't trigger a reload). We can do that by adding an appropriate timeout and catching/swallowing it.
                await _page.WaitForNavigationAsync(new NavigationOptions
                {
                    Timeout = 5000
                });

                var failureReason = await DetermineExportFailureReason(account);
                _logger.LogWarning("Page reported an error when exporting the statement: {error}", failureReason);
                return DownloadResult.Failed(failureReason);
            }
            catch (TimeoutException)
            {
                // swallowed on purpose, we will only end up here if statement export was successful
            }
            catch (WaitTaskTimeoutException e)
            {
                _logger.LogError(e, "Export failed, but site did not report a reason in time.");
                // Hit an error, but page took too long to report it
                return DownloadResult.Failed("Timeout");
            }
            catch (Exception)
            {
                var failureReason = await DetermineExportFailureReason(account);
                _logger.LogWarning("Page reported an error when exporting the statement: {error}", failureReason);
                return DownloadResult.Failed(failureReason);
            }

            return DownloadResult.Success;
        }

        private async Task<string?> DetermineExportFailureReason(Account account)
        {
            var uri = new Uri(_page.Url);
            var pageReportedError = HttpUtility.ParseQueryString(uri.Query).AllKeys.Any(x => x.Equals("failedExport", StringComparison.InvariantCultureIgnoreCase));
            if (!pageReportedError)
            {
                return null;
            }

            await TakeScreenshotIfEnabled($"{account.Identifier.StripIllegalCharacters()} - Statement export page after export failure");

            var failureReason = HttpUtility.ParseQueryString(uri.Query)["errorMessage"];
            return failureReason;
        }

        private async Task TakeScreenshotIfEnabled(string context)
        {
            if (!_takeScreenshots)
                return;

            Guard.AgainstNullOrEmpty(context, nameof(context));

            var path = Path.Combine(_screenshotOutputPath, $"{_diagnosticsSessionId} {context}.jpg");
            await _page.ScreenshotAsync(path);
        }

        public async ValueTask DisposeAsync()
        {
            _page?.Dispose();
            if (_browser != null)
            {
                await _browser.CloseAsync();
                _browser.Dispose();
            }
        }

        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }
    }
}
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
