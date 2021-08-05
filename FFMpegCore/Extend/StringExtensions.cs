namespace FFMpegCore.Extend
{
    internal static class StringExtensions
    {
        public static string EncloseIfContainsSpace(this string input)
        {
            return input.Contains(" ") ? $"'{input}'" : input;
        }
    }
}