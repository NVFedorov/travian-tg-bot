using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TTB.Common.Extensions
{
    public static class TimeSpanExtensions
    {
        public static readonly string TimeSpanFormat = @"hh\:mm\:ss";

        public static TimeSpan ToTimeSpan(this string source)
        {
            return TimeSpan.ParseExact(source, TimeSpanFormat, CultureInfo.InvariantCulture);
        }
    }
}
