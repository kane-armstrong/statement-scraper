using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("StatementDeserializer.UnitTests")]
namespace StatementDeserializer.Extensions;

internal static class StringExtensions
{
    private const string QuotationMark = "\"";

    public static string RemoveQuotationMarks(this string s)
    {
        return s.Replace(QuotationMark, "");
    }

    private const string ParenthesesPattern = "[()]";

    public static string RemoveParentheses(this string s)
    {
        return Regex.Replace(s, ParenthesesPattern, "");
    }
}