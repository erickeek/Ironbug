using System.Linq;

namespace IronBug.Helpers
{
    public static class BooleanHelper
    {
        public static bool ToBoolean(this string value)
        {
            return value != null && new[] { "1", "true", "on", "yes", "y", "sim", "s" }.Contains(value.ToLowerInvariant());
        }
    }
}