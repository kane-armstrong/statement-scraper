using System;

namespace StatementScraper.Extensions;

internal static class StringExtensions
{
    public static string Garble(this string source, short shift)
    {
        var max = Convert.ToInt32(char.MaxValue);
        var min = Convert.ToInt32(char.MinValue);
        var buffer = source.ToCharArray();
        for (var i = 0; i < buffer.Length; i++)
        {
            var shifted = Convert.ToInt32(buffer[i]) + shift;
            if (shifted > max) shifted -= max;
            else if (shifted < min) shifted += max;
            buffer[i] = Convert.ToChar(shifted);
        }
        return new string(buffer);
    }
}