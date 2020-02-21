using StatementScraper.Extensions;

namespace StatementScraper
{
    internal static class ElementSelectors
    {
        public static string LoginButton => Caesar(".wzrtyMy");
        public static string UserNameInput = Caesar(".o`~p}ylxp");
        public static string PasswordInput = Caesar(".{l~~z}o");

        public static string StatementsPageLink = Caesar(".~lpxpy~jwtyv");

        public static string AccountsSelectList = Caesar(".]p|p~jLnnzyVp");
        public static string ExportFormatSelectList = Caesar(".]p|p~jP{z}Qz}xl");

        public static string ExportButtonContainer = Caesar(".^lpxpyW[P{z}");

        public static string FromDateInputDay = Caesar(".]p|p~jQ}zxOlpjty{Olp");
        public static string FromDateInputMonth = Caesar(".]p|p~jQ}zxOlpjty{Xzys");
        public static string FromDateInputYear = Caesar(".]p|p~jQ}zxOlpjty{dpl}");

        public static string ToDateInputDay = Caesar(".]p|p~j_zOlpjty{Olp");
        public static string ToDateInputMonth = Caesar(".]p|p~j_zOlpjty{Xzys");
        public static string ToDateInputYear = Caesar(".]p|p~j_zOlpjty{dpl}");

        private const short Shift = 2 ^ 15 - 1 ^ 7;
        private static string Caesar(string input) => input.Caesar(-Shift);
    }
}
