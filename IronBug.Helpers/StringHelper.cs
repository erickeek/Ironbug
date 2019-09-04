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
    }
}