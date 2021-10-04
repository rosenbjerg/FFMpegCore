using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FFMpegCore.Enums;
using FFMpegCore.Exceptions;

namespace FFMpegCore.Arguments
{
    public class AudioFiltersArgument : IArgument
    {
        public readonly AudioFilterOptions Options;

        public AudioFiltersArgument(AudioFilterOptions options)
        {
            Options = options;
        }

        public string Text => GetText();

        private string GetText()
        {
            if (!Options.Arguments.Any())
                throw new FFMpegArgumentException("No audio-filter arguments provided");

            var arguments = Options.Arguments
                .Where(arg => !string.IsNullOrEmpty(arg.Value))
                .Select(arg =>
                {
                    var escapedValue = arg.Value.Replace(",", "\\,");
                    return string.IsNullOrEmpty(arg.Key) ? escapedValue : $"{arg.Key}={escapedValue}";
                });

            return $"-af \"{string.Join(", ", arguments)}\"";
        }
    }

    public interface IAudioFilterArgument
    {
        public string Key { get; }
        public string Value { get; }
    }

    public class AudioFilterOptions
    {
        public List<IAudioFilterArgument> Arguments { get; } = new List<IAudioFilterArgument>();
     
        private AudioFilterOptions WithArgument(IAudioFilterArgument argument)
        {
            Arguments.Add(argument);
            return this;
        }
    }
}