using System;
using System.Globalization;

namespace IronBug.Helpers
{
    public static class DecimalHelper
    {
        public static decimal ToDecimal(this string value, decimal defaultValue = 0)
        {
            return value.ToDecimal(CultureInfo.InvariantCulture, defaultValue);
        }

        public static decimal ToDecimal(this string value, CultureInfo culture, decimal defaultValue = 0)
        {
            try
            {
                return Convert.ToDecimal(value, culture);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}