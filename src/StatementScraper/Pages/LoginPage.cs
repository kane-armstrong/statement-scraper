using PuppeteerSharp;
using StatementScraper.Extensions;
using System;
using System.Threading.Tasks;

namespace StatementScraper.Pages
{
    internal class LoginPage
    {
        private const string Address = "https://online.asb.co.nz/auth";

        private readonly Page _page;

        public LoginPage(Page page)
        {
            _page = page;
        }

        public async Task BrowseTo()
        {
            await _page.GoToAsync(Address);
        }

        public async Task Login(string username, string password)
        {
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
                throw new Exception("Invalid username or password");
            }
        }
    }
}
