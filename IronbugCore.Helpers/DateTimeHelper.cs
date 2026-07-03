using System.Globalization;

namespace IronbugCore.Helpers;

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
        if (value == null) return defaultValue;
        var str = value.ToString();
        return DateTime.TryParse(str, out var result) ? result : defaultValue;
    }

    public static DateTime ToDateTime(this object value, DateTime defaultValue, CultureInfo culture)
    {
        if (value == null) return defaultValue;
        var str = value.ToString();
        return DateTime.TryParse(str, culture, DateTimeStyles.None, out var result) ? result : defaultValue;
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
        if (value == null || value is string s && string.IsNullOrEmpty(s))
            return null;

        var str = value.ToString();
        if (DateTime.TryParse(str, culture, DateTimeStyles.None, out var result))
            return result;

        return null;
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

    private static readonly TimeZoneInfo? BrasiliaTimeZone = ResolveBrasiliaTimeZone();

    public static DateTime HorarioDeBrasilia(this DateTime data)
    {
        return BrasiliaTimeZone == null ? data : TimeZoneInfo.ConvertTime(data, BrasiliaTimeZone);
    }

    private static TimeZoneInfo? ResolveBrasiliaTimeZone()
    {
        // "E. South America Standard Time" (Windows) / "America/Sao_Paulo" (Linux/macOS).
        // Resolvido uma única vez — o fuso não muda em tempo de execução.
        foreach (var tzId in new[] { "E. South America Standard Time", "America/Sao_Paulo" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(tzId);
            }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }

        return null;
    }
}