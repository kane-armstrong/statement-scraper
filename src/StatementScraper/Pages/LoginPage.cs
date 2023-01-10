using PuppeteerSharp;
using StatementScraper.Extensions;
using System;
using System.Threading.Tasks;

namespace StatementScraper.Pages;

internal class LoginPage
{
    private const string Address = "s\u007f\u007f{~E::zywtyp9l~m9nz9y\u0085:l\u0080\u007fs";

    private readonly IPage _page;

    public LoginPage(IPage page)
    {
        _page = page;
    }

    public async Task<BalancesPage> Login(string username, string password)
    {
        await _page.GoToAsync(Address.Garble(-(2 ^ 15 - 1 ^ 7)));

        var usernameInput = await _page.GetElement(ElementSelectors.UserNameInput);
        await usernameInput.TypeAsync(username);

        var passwordInput = await _page.GetElement(ElementSelectors.PasswordInput);
        await passwordInput.TypeAsync(password);

        var loginButton = await _page.GetElement(ElementSelectors.LoginButton);
        await loginButton.ClickAsync();
        await _page.WaitForNavigationAsync(new NavigationOptions
        {
            Timeout = 5000
        });

        var loginButtonAgain = await _page.GetElementOrDefault(ElementSelectors.LoginButton);
        if (loginButtonAgain != null)
        {
            throw new InvalidOperationException("Invalid username or password");
        }

        return new BalancesPage(_page);
    }
}