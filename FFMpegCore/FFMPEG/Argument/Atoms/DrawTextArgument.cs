using System.Collections.Generic;
using System.Linq;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Drawtext video filter argument
    /// </summary>
    public class DrawTextArgument : Argument<IEnumerable<(string key, string value)>>
    {
        public DrawTextArgument(string text, string fontPath, params (string, string)[] optionalArguments) 
            : base(new[] {("text", text), ("fontfile", fontPath)}.Concat(optionalArguments))
        {
        }

        public override string GetStringValue()
        {
            return $"-vf drawtext=\"{string.Join(": ", Value.Select(FormatArgumentPair))}\" ";
        }

        private static string FormatArgumentPair((string key, string value) pair)
        {
            return $"{pair.key}={EncloseIfContainsSpace(pair.value)}";
        }

        private static string EncloseIfContainsSpace(string input)
        {
            return input.Contains(" ") ? $"'{input}'" : input;
        }
    }
}