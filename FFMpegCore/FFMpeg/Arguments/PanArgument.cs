using System;
using System.Linq;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents scale parameter
    /// </summary>
    public class PanArgument : IAudioFilterArgument
    {
        public readonly string ChannelLayout;
        private readonly string[] _outputDefinitions;

        public PanArgument(string channelLayout, params string[] outputDefinitions)
        {
            if (string.IsNullOrWhiteSpace(channelLayout))
            {
                throw new ArgumentException("The channel layout must be set" ,nameof(channelLayout));
            }

            ChannelLayout = channelLayout;
            
            _outputDefinitions = outputDefinitions;
        }

        public PanArgument(int channels, params string[] outputDefinitions)
        {
            if (channels <= 0) throw new ArgumentOutOfRangeException(nameof(channels));
            
            if (outputDefinitions.Length > channels)
                throw new ArgumentException("The number of output definitions must be equal or lower than number of channels", nameof(outputDefinitions));
            
            ChannelLayout = $"{channels}c";

            _outputDefinitions = outputDefinitions;
        }

        public string Key { get; } = "pan";

        public string Value =>
            string.Join("|", Enumerable.Empty<string>().Append(ChannelLayout).Concat(_outputDefinitions));
    }
}
