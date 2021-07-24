using Contracts;
using PuppeteerSharp;
using StatementScraper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace StatementScraper.Pages
{
    internal class ExportStatementPage
    {
        private readonly Page _page;

        public ExportStatementPage(Page page)
        {
            _page = page;
        }

        public async Task<IEnumerable<ScrapedAccount>> ListAccounts()
        {
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

        public async Task<DownloadResult> DownloadStatement(Account account, DateTimeOffset start, DateTimeOffset end, string format)
        {
            await _page.TypeInSelectedElement(ElementSelectors.FromDateInputDay, start.Day.ToString());
            await _page.TypeInSelectedElement(ElementSelectors.FromDateInputMonth, start.Month.ToString());
            await _page.TypeInSelectedElement(ElementSelectors.FromDateInputYear, start.Year.ToString());

            await _page.TypeInSelectedElement(ElementSelectors.ToDateInputDay, end.Day.ToString());
            await _page.TypeInSelectedElement(ElementSelectors.ToDateInputMonth, end.Month.ToString());
            await _page.TypeInSelectedElement(ElementSelectors.ToDateInputYear, end.Year.ToString());

            var exportButtonDiv = await _page.GetElement(ElementSelectors.ExportButtonContainer);

            await _page.SelectAsync(ElementSelectors.AccountsSelectList, account.Identifier);
            await _page.SelectAsync(ElementSelectors.ExportFormatSelectList, format);

            try
            {
                await exportButtonDiv.ClickAsync();

                // If there is no statement for the selected period then the page reloads and reports an error. We're waiting
                // here to handle those cases - not waiting will break things. But waiting will also break things when export
                // is successful, hence the catch and swallow TimeoutException thing
                await _page.WaitForNavigationAsync(new NavigationOptions
                {
                    Timeout = 5000
                });

                var failureReason = DetermineExportFailureReason();
                return DownloadResult.Failed(failureReason);
            }
            catch (TimeoutException)
            {
                // swallowed on purpose, we will only end up here if statement export was successful
            }
            catch (WaitTaskTimeoutException)
            {
                // Hit an error, but page took too long to report it
                return DownloadResult.Failed("Timeout");
            }
            catch (Exception)
            {
                var failureReason = DetermineExportFailureReason();
                return DownloadResult.Failed(failureReason);
            }

            return DownloadResult.Success;
        }

        private string DetermineExportFailureReason()
        {
            var uri = new Uri(_page.Url);
            var pageReportedError = HttpUtility.ParseQueryString(uri.Query).AllKeys.Any(x => x.Equals("failedExport", StringComparison.InvariantCultureIgnoreCase));
            if (!pageReportedError)
            {
                return "Unknown";
            }

            var failureReason = HttpUtility.ParseQueryString(uri.Query)["errorMessage"];
            return failureReason;
        }
    }
}
