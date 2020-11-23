using System;
using System.Globalization;
using TTB.Common.Settings;

namespace TTB.Common.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        public static readonly string TimestampFormat = "yyyyMMddHHmmssfff";
        public static readonly string DateTimeFormat = "yyyy-MM-ddTHH:mm:sszzzz";
        public static readonly string DateTimeFormatWithMills = "yyyy-MM-ddTHH:mm:ss.fffzzzz";
        public static readonly string ReadableDateTimeFormat = "HH:mm:ss dd.MM.yyyy";

        public static readonly string[] ParseFormats =
        {
            DateTimeFormat,
            "dd.MM.yy, HH:mm:ss",
            TimestampFormat,
            DateTimeFormatWithMills,
            ReadableDateTimeFormat
        };

        public static string ToDisplayString(this DateTimeOffset dateTimeOffset, TimeZoneInfo tz = null)
        {
            var display = TimeZoneInfo.ConvertTimeFromUtc(dateTimeOffset.UtcDateTime, tz);
            return display.ToString(ReadableDateTimeFormat);
        }

        public static string ToFormattedString(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToString(DateTimeFormat);
        }

        public static string ToFormattedString(this DateTime? dateTime)
        {
            if (!dateTime.HasValue) return string.Empty;
            return dateTime.Value.ToString(DateTimeFormat);
        }

        public static string ToDisplayStringApplyTimeZone(this DateTimeOffset dateTimeOffset, string timezone)
        {
            var hours = timezone.Contains("+") ? timezone.Replace("UTC+", string.Empty) : timezone.Replace("UTC", string.Empty);
            var offset = TimeSpan.FromHours(int.Parse(hours));
            dateTimeOffset = dateTimeOffset.ToOffset(offset);

            return dateTimeOffset.ToString(ReadableDateTimeFormat);
        }

        public static string ToFormattedStringApplyTimeZone(this DateTimeOffset dateTimeOffset, string timezone)
        {
            var hours = timezone.Contains("+") ? timezone.Replace("UTC+", string.Empty) : timezone.Replace("UTC", string.Empty);
            var offset = TimeSpan.FromHours(int.Parse(hours));
            dateTimeOffset = dateTimeOffset.ToOffset(offset);
            return dateTimeOffset.ToString(DateTimeFormat);
        }

        public static string ToTimeStamp(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToString(TimestampFormat);
        }

        public static string ToFormattedStringWithMills(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToString(DateTimeFormatWithMills);
        }
    }
}
