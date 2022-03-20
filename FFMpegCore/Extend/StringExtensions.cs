using System;
using System.Collections.Generic;
using System.Text;

namespace FFMpegCore.Extend
{
    internal static class StringExtensions
    {
        private static Dictionary<char, string> CharactersSubstitution { get; } = new Dictionary<char, string>
        {
            { '\\', @"\\" },
            { ':', @"\:" },
            { '[', @"\[" },
            { ']', @"\]" },
            { '\'', @"'\\\''" }
        };

        /// <summary>
        ///     Enclose string between quotes if contains an space character
        /// </summary>
        /// <param name="input">The input</param>
        /// <returns>The enclosed string</returns>
        public static string EncloseIfContainsSpace(string input)
        {
            return input.Contains(" ") ? $"'{input}'" : input;
        }

        /// <summary>
        /// Enclose an string in quotes
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string EncloseInQuotes(string input)
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
        public static string ToFFmpegLibavfilterPath(string source)
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

        /// <summary>
        /// Counts the number of occurrences of the specified substring within
        /// the current string.
        /// </summary>
        /// <param name="s">The current string.</param>
        /// <param name="substring">The substring we are searching for.</param>
        /// <param name="aggressiveSearch">Indicates whether or not the algorithm 
        /// should be aggressive in its search behavior (see Remarks). Default 
        /// behavior is non-aggressive.</param>
        /// <remarks>This algorithm has two search modes - aggressive and 
        /// non-aggressive. When in aggressive search mode (aggressiveSearch = 
        /// true), the algorithm will try to match at every possible starting 
        /// character index within the string. When false, all subsequent 
        /// character indexes within a substring match will not be evaluated. 
        /// For example, if the string was 'abbbc' and we were searching for 
        /// the substring 'bb', then aggressive search would find 2 matches 
        /// with starting indexes of 1 and 2. Non aggressive search would find 
        /// just 1 match with starting index at 1. After the match was made, 
        /// the non aggressive search would attempt to make it's next match 
        /// starting at index 3 instead of 2.</remarks>
        /// <returns>The count of occurrences of the substring within the string.</returns>
        public static int CountOccurrences(this string s, string substring,
            bool aggressiveSearch = false)
        {
            // if s or substring is null or empty, substring cannot be found in s
            // if the length of substring is greater than the length of s,
            // substring cannot be found in s
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(substring) || substring.Length > s.Length)
                return 0;

            int count = 0, n = 0;
            while ((n = s.IndexOf(substring, n, StringComparison.InvariantCulture)) != -1)
            {
                if (aggressiveSearch)
                    n++;
                else
                    n += substring.Length;
                count++;
            }

            return count;
        }
    }
}