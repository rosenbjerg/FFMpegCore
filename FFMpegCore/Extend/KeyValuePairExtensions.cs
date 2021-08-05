using System.Collections.Generic;

namespace FFMpegCore.Extend
{
    internal static class KeyValuePairExtensions
    {
        /// <summary>
        ///     Concat the two members of a <see cref="KeyValuePair{TKey,TValue}" />
        /// </summary>
        /// <param name="pair">Input object</param>
        /// <param name="enclose">
        ///     If true encloses the value part between quotes if contains an space character. If false use the
        ///     value unmodified
        /// </param>
        /// <returns>The formatted string</returns>
        public static string FormatArgumentPair(this KeyValuePair<string, string> pair, bool enclose)
        {
            var key = pair.Key;
            var value = enclose ? pair.Value.EncloseIfContainsSpace() : pair.Value;

            return $"{key}={value}";
        }
    }
}