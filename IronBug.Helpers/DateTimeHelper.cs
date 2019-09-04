using System;
using System.Globalization;

namespace IronBug.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime ToDateTime(this object value)
        {
            return Convert.ToDateTime(value);
        }

        public static DateTime ToDateTime(this object value, CultureInfo culture)
        {
            return Convert.ToDateTime(value, culture);
        }

        public static DateTime ToDateTime(this object value, DateTime defaultValue)
        {
            try
            {
                return Convert.ToDateTime(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static DateTime ToDateTime(this object value, DateTime defaultValue, CultureInfo culture)
        {
            try
            {
                return Convert.ToDateTime(value, culture);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static DateTime? ToDateTimeOrNull(this string text, string format)
        {
            var isParsed = DateTime.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value);
            if (isParsed && value.Year >= 1753)
                return value;

            return null;
        }

        public static DateTime? ToDateTimeOrNull(this string text)
        {
            var isParsed = DateTime.TryParse(text, out var value);
            if (isParsed && value.Year >= 1753)
                return value;

            return null;
        }

        public static DateTime? ToDateTimeOrNull(this object value, CultureInfo culture)
        {
            try
            {
                if (value == null || value is string s && s == "")
                    return null;

                return Convert.ToDateTime(value, culture);
            }
            catch
            {
                return null;
            }
        }

        public static DateTime GetFirstDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime GetLastDayOfMonth(this DateTime date)
        {
            var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            return new DateTime(date.Year, date.Month, daysInMonth);
        }

        public static string ToShortDateString(this DateTime? date)
        {
            return !date.HasValue ? string.Empty : date.Value.ToShortDateString();
        }
    }
}