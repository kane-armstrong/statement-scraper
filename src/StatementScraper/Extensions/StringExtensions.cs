using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StatementScraper.Extensions
{
    internal static class StringExtensions
    {
        private static readonly IEnumerable<char> IllegalCharacters = 
            Path.GetInvalidFileNameChars()
            .Union(new[] {':'});
        
        public static string StripIllegalCharacters(this string input) =>
            IllegalCharacters.Aggregate(input, (current, c) => current.Replace(c.ToString(), ""));

        public static string Caesar(this string source, short shift)
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
}