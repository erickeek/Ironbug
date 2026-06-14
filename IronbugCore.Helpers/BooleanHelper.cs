using System.Collections.Frozen;

namespace IronbugCore.Helpers;

public static class BooleanHelper
{
    private static readonly FrozenSet<string> TrueValues =
        new[] { "1", "true", "on", "yes", "y", "sim", "s" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static bool ToBoolean(this string value)
    {
        return value != null && TrueValues.Contains(value);
    }
}
