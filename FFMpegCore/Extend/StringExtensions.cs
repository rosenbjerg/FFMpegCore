using System.Collections.Generic;
using System.Text;

namespace FFMpegCore.Extend
{
    internal static class StringExtensions
    {
        private static Dictionary<char, string> CharactersSubstitution { get; } = new Dictionary<char, string>
        {
            {'\\', @"\\"},
            {':', @"\:"},
            {'[', @"\["},
            {']', @"\]"},
            // {'\'', @"\'"} TODO: Quotes need to be escaped but i failed miserably
        };

        /// <summary>
        ///     Enclose string between quotes if contains an space character
        /// </summary>
        /// <param name="input">The input</param>
        /// <returns>The enclosed string</returns>
        public static string EncloseIfContainsSpace(this string input)
        {
            return input.Contains(" ") ? $"'{input}'" : input;
        }

        /// <summary>
        /// Enclose an string in quotes
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string EncloseInQuotes(this string input)
        {
            return $"'{input}'";
        }

        /// <summary>
        ///     Scape several characters in subtitle path used by FFmpeg
        /// </summary>
        /// <remarks>
        ///     This is needed because internally FFmpeg use Libav Filters
        ///     and the info send to it must be in an specific format
        /// </remarks>
        /// <param name="source"></param>
        /// <returns>Scaped path</returns>
        public static string ToFFmpegLibavfilterPath(this string source)
        {
            return source.Replace(CharactersSubstitution);
        }

        public static string Replace(this string str, Dictionary<char, string> replaceList)
        {
            var parsedString = new StringBuilder();

            foreach (var l in str)
            {
                if (replaceList.ContainsKey(l))
                {
                    parsedString.Append(replaceList[l]);
                }
                else
                {
                    parsedString.Append(l);
                }
            }

            return parsedString.ToString();
        }
    }
}