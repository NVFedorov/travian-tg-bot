using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TTB.Common.Extensions
{
    public static class StringExtensions
    {
        public static TimeSpan ToOffset(this string timezone)
        {
            var hours = timezone.Contains("+") ? timezone.Replace("UTC+", string.Empty) : timezone.Replace("UTC", string.Empty);
            var offset = TimeSpan.FromHours(int.Parse(hours));
            return offset;
        }

        public static DateTime? ToDateTime(this string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            return DateTime.ParseExact(input, DateTimeOffsetExtensions.ParseFormats, CultureInfo.InvariantCulture);
        }

        public static DateTimeOffset ToDateTimeOffset(this string input)
        {
            return DateTimeOffset.ParseExact(input, DateTimeOffsetExtensions.ParseFormats, CultureInfo.InvariantCulture);
        }
    }
}
