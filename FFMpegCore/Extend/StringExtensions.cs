namespace FFMpegCore.Extend
{
    internal static class StringExtensions
    {
        /// <summary>
        ///     Enclose string between quotes if contains an space character
        /// </summary>
        /// <param name="input">The input</param>
        /// <returns>The enclosed string</returns>
        public static string EncloseIfContainsSpace(this string input)
        {
            return input.Contains(" ") ? $"'{input}'" : input;
        }
    }
}