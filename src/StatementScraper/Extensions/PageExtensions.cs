using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace StatementScraper.Extensions;

internal static class PageExtensions
{
    public static async Task<IElementHandle> GetElement(this IPage page, string name, int timeout = 5000)
    {
        try
        {
            return await page.WaitForSelectorAsync(name, new WaitForSelectorOptions
            {
                Timeout = timeout
            });
        }
        catch (Exception e)
        {
            throw new ElementNotFoundException(name, "An error occurred while attempting to get the element", e);
        }
    }

    public static async Task<IElementHandle?> GetElementOrDefault(this IPage page, string name, int timeout = 5000)
    {
        try
        {
            return await page.WaitForSelectorAsync(name, new WaitForSelectorOptions
            {
                Timeout = timeout
            });
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static async Task TypeInSelectedElement(this IPage page, string selector, string input, int timeout = 5000)
    {
        var element = await page.GetElement(selector, timeout);
        await element.TypeAsync(input);
    }

    // Using IAsyncEnumerable<T> here leads to errors when the caller is enumerating, this throws partway into
    // the enumeration, and the Page instance is shared between this and the caller.
    public static async Task<IEnumerable<string>> GetSelectListOptions(this IPage page, string selector)
    {
        var selectElement = await page.GetElement(selector);
        var properties = await selectElement.GetPropertiesAsync();
        // valid select list items appear in props as indexed ints
        var relevantSelectOptions = properties.Where(x => char.IsDigit(x.Key[0])).Select(x => x.Value);

        var results = new List<string>();
        foreach (var result in relevantSelectOptions)
        {
            var valueHandle = await result.GetPropertyAsync("value");
            var value = (string)(await valueHandle.JsonValueAsync());
            results.Add(value);
        }

        return results;
    }

    public static async Task SetDownloadPath(this IPage page, string path)
    {
        await page.Client.SendAsync("Page.setDownloadBehavior", new
        {
            behavior = "allow",
            downloadPath = path
        });
    }
}