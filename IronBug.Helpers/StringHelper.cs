using System;

namespace IronBug.Helpers
{
    public static class StringHelper
    {
        public static string Truncate(this string input, int length, bool concatWithReticences = true)
        {
            if (input == null)
                return string.Empty;

            if (input.Length <= length)
                return input;

            input = input.Substring(0, length);

            if (concatWithReticences)
                input += "...";

            return input;
        }

        public static string ReplaceFirstOccurance(this string source, string find, string replace)
        {
            var startIndex = source.IndexOf(find, StringComparison.Ordinal);
            return source.Remove(startIndex, find.Length).Insert(startIndex, replace);
        }

        public static string ReplaceLastOccurance(this string source, string find, string replace)
        {
            var startIndex = source.LastIndexOf(find, StringComparison.Ordinal);
            return startIndex > 0 ? source.Remove(startIndex, find.Length).Insert(startIndex, replace) : source;
        }
    }
}