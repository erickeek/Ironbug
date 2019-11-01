using System;
using System.Globalization;
using System.Linq;
using System.Text;

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

        public static string ToLowerFirstLetter(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            return input.Substring(0, 1).ToLower() + input.Substring(1);
        }

        public static string ToUpperFirstLetter(this string input, bool onlyFirstLetterUpper = true)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            if (onlyFirstLetterUpper)
                input = input.ToLower();

            return input.Substring(0, 1).ToUpper() + input.Substring(1);
        }

        public static string RemoveAccents(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var str = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(input.Length);
            foreach (var ch in str.Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark))
            {
                stringBuilder.Append(ch);
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string Nl2Br(this string input)
        {
            return input.Replace("\r\n", "<br />").Replace("\n", "<br />");
        }
    }
}