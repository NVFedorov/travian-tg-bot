using System;
using System.Collections.Generic;
using System.Text;

namespace TTB.Common.Settings
{
    public static class TimeZoneMapping
    {
        public static Dictionary<string, TimeZoneInfo> Mapping = new Dictionary<string, TimeZoneInfo>
        {
            { "Europe/Moscow", TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time") },
            { "UTC-12", TimeZoneInfo.FindSystemTimeZoneById("Dateline Standard Time") },
            { "UTC-11", TimeZoneInfo.FindSystemTimeZoneById("UTC-11") },
            { "UTC-10", TimeZoneInfo.FindSystemTimeZoneById("Aleutian Standard Time") },
            { "UTC-9", TimeZoneInfo.FindSystemTimeZoneById("UTC-09") },
            { "UTC-8", TimeZoneInfo.FindSystemTimeZoneById("UTC-08") },
            { "UTC-7", TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time") },
            { "UTC-6", TimeZoneInfo.FindSystemTimeZoneById("Central America Standard") },
            { "UTC-5", TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") },
            { "UTC-4", TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time") },
            { "UTC-3", TimeZoneInfo.FindSystemTimeZoneById("SA Eastern Standard Time") },
            { "UTC-2", TimeZoneInfo.FindSystemTimeZoneById("UTC-02") },
            { "UTC-1", TimeZoneInfo.FindSystemTimeZoneById("Azores Standard Time") },
            { "UTC", TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time") },
            { "UTC+1", TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time") },
            { "UTC+2", TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time") },
            { "UTC+3", TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time") },
            { "UTC+4", TimeZoneInfo.FindSystemTimeZoneById("Russia Time Zone 3") },
            { "UTC+5", TimeZoneInfo.FindSystemTimeZoneById("Ekaterinburg Standard Time") },
            { "UTC+6", TimeZoneInfo.FindSystemTimeZoneById("Central Asia Standard Time") },
            { "UTC+7", TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time") },
            { "UTC+8", TimeZoneInfo.FindSystemTimeZoneById("North Asia East Standard Time") },
            { "UTC+9", TimeZoneInfo.FindSystemTimeZoneById("Transbaikal Standard Time") },
            { "UTC+10", TimeZoneInfo.FindSystemTimeZoneById("West Pacific Standard Time") },
            { "UTC+11", TimeZoneInfo.FindSystemTimeZoneById("Russia Time Zone 10") },
            { "UTC+12", TimeZoneInfo.FindSystemTimeZoneById("UTC+12") },
            { "UTC+13", TimeZoneInfo.FindSystemTimeZoneById("UTC+13") },
            { "UTC+14", TimeZoneInfo.FindSystemTimeZoneById("Line Islands Standard Time") },
        };
    }
}
