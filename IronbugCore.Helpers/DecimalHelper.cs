using System.Globalization;

namespace IronbugCore.Helpers;

public static class DecimalHelper
{
    public static decimal ToDecimal(this string value, decimal defaultValue = 0)
    {
        return value.ToDecimal(CultureInfo.InvariantCulture, defaultValue);
    }

    public static decimal ToDecimal(this string value, CultureInfo culture, decimal defaultValue = 0)
    {
        return decimal.TryParse(value, NumberStyles.Any, culture, out var result) ? result : defaultValue;
    }

    public static string ToString(this decimal? value, string format, string defaultValue = "")
    {
        return value == null ? defaultValue : value.Value.ToString(format);
    }
}