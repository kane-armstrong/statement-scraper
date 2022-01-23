using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("StatementDeserializer.UnitTests")]
namespace StatementDeserializer.Extensions;

internal static class StringExtensions
{
    private const string QuotationMark = "\"";

    public static string RemoveQuotationMarks(this string @this)
    {
        return @this.Replace(QuotationMark, "");
    }

    private const string ParenthesesPattern = "[()]";

    public static string RemoveParentheses(this string @this)
    {
        return Regex.Replace(@this, ParenthesesPattern, "");
    }
}