using StatementScraper.Extensions;

namespace StatementScraper
{
    internal static class ElementSelectors
    {
        public static string LoginButton => Garble(".wzrtyMy");
        public static string UserNameInput = Garble(".o`~p}ylxp");
        public static string PasswordInput = Garble(".{l~~z}o");

        public static string StatementsPageLink = Garble(".~lpxpy~jwtyv");

        public static string AccountsSelectList = Garble(".]p|p~jLnnzyVp");
        public static string ExportFormatSelectList = Garble(".]p|p~jP{z}Qz}xl");

        public static string ExportButtonContainer = Garble(".^lpxpyW[P{z}");

        public static string FromDateInputDay = Garble(".]p|p~jQ}zxOlpjty{Olp");
        public static string FromDateInputMonth = Garble(".]p|p~jQ}zxOlpjty{Xzys");
        public static string FromDateInputYear = Garble(".]p|p~jQ}zxOlpjty{dpl}");

        public static string ToDateInputDay = Garble(".]p|p~j_zOlpjty{Olp");
        public static string ToDateInputMonth = Garble(".]p|p~j_zOlpjty{Xzys");
        public static string ToDateInputYear = Garble(".]p|p~j_zOlpjty{dpl}");

        private const short Shift = 2 ^ 15 - 1 ^ 7;
        private static string Garble(string input) => input.Garble(-Shift);
    }
}
