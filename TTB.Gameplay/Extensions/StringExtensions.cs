using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TTB.Gameplay.Extensions
{
    public static class StringExtensions
    {
        private const string replaceRegexp = @"[^\u0000-\u007F]+";

        public static string RemoveNonAsciiCharacters(this string source)
        {
            return Regex.Replace(source, replaceRegexp, string.Empty);
        }
    }
}
