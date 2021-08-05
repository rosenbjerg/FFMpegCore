using System.Collections.Generic;

namespace FFMpegCore.Extend
{
    internal static class KeyValuePairExtensions
    {
        public static string FormatArgumentPair(this KeyValuePair<string, string> pair, bool enclose)
        {
            var key = pair.Key;
            var value = enclose ? pair.Value.EncloseIfContainsSpace() : pair.Value;

            return $"{key}={value}";
        }
    }
}