using System.Globalization;
using System.Text;

namespace IronbugCore.Helpers;

public static class StringHelper
{
    public static string Truncate(this string input, int length, bool concatWithEllipses = true)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= length)
            return input ?? string.Empty;

        return concatWithEllipses
            ? string.Concat(input.AsSpan(0, length), "...")
            : input[..length];
    }

    public static string ReplaceFirstOccurrence(this string source, string find, string replace)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(find))
            return source;

        var startIndex = source.IndexOf(find, StringComparison.Ordinal);
        if (startIndex < 0)
            return source;

        return string.Concat(source.AsSpan(0, startIndex), replace, source.AsSpan(startIndex + find.Length));
    }

    public static string ReplaceLastOccurrence(this string source, string find, string replace)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(find))
            return source;

        var startIndex = source.LastIndexOf(find, StringComparison.Ordinal);
        return startIndex >= 0
            ? string.Concat(source.AsSpan(0, startIndex), replace, source.AsSpan(startIndex + find.Length))
            : source;
    }

    public static string ToLowerFirstLetter(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        if (char.IsLower(input[0]))
            return input;

        return string.Create(input.Length, input, (span, str) =>
        {
            str.AsSpan().CopyTo(span);
            span[0] = char.ToLowerInvariant(str[0]);
        });
    }

    public static string ToUpperFirstLetter(this string input, bool onlyFirstLetterUpper = true)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        if (onlyFirstLetterUpper)
        {
            return string.Create(input.Length, input, (span, str) =>
            {
                for (var i = 0; i < span.Length; i++)
                    span[i] = char.ToLower(str[i]);
                span[0] = char.ToUpperInvariant(span[0]);
            });
        }

        if (char.IsUpper(input[0]))
            return input;

        return string.Create(input.Length, input, (span, str) =>
        {
            str.AsSpan().CopyTo(span);
            span[0] = char.ToUpperInvariant(str[0]);
        });
    }

    public static string RemoveAccents(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var normalizedString = input.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(input.Length);

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string Nl2Br(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return input.Replace("\r\n", "<br />").Replace("\n", "<br />");
    }

    public static string ToBase64(this string s)
    {
        if (string.IsNullOrEmpty(s))
            return string.Empty;

        var buffer = Encoding.UTF8.GetBytes(s);
        return Convert.ToBase64String(buffer);
    }

    public static string OnlyNumbers(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (char.IsDigit(c))
                sb.Append(c);
        }

        return sb.ToString();
    }
}