namespace FFMpegCore.Arguments
{
    /// <summary>
    ///     Mix channels with specific gain levels.
    /// </summary>
    public class PanArgument : IAudioFilterArgument
    {
        public readonly string ChannelLayout;
        private readonly string[] _outputDefinitions;

        /// <summary>
        ///     Mix channels with specific gain levels <see href="https://ffmpeg.org/ffmpeg-filters.html#toc-pan-1"/>
        /// </summary>
        /// <param name="channelLayout">
        ///     Represent the output channel layout. Like "stereo", "mono", "2.1", "5.1"
        /// </param>
        /// <param name="outputDefinitions">
        ///     Output channel specification, of the form: "out_name=[gain*]in_name[(+-)[gain*]in_name...]"
        /// </param>
        public PanArgument(string channelLayout, params string[] outputDefinitions)
        {
            if (string.IsNullOrWhiteSpace(channelLayout))
            {
                throw new ArgumentException("The channel layout must be set", nameof(channelLayout));
            }

            ChannelLayout = channelLayout;

            _outputDefinitions = outputDefinitions;
        }

        /// <summary>
        ///     Mix channels with specific gain levels <see href="https://ffmpeg.org/ffmpeg-filters.html#toc-pan-1"/>
        /// </summary>
        /// <param name="channels">Number of channels in output file</param>
        /// <param name="outputDefinitions">
        ///     Output channel specification, of the form: "out_name=[gain*]in_name[(+-)[gain*]in_name...]"
        /// </param>
        public PanArgument(int channels, params string[] outputDefinitions)
        {
            if (channels <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(channels));
            }

            if (outputDefinitions.Length > channels)
            {
                throw new ArgumentException("The number of output definitions must be equal or lower than number of channels", nameof(outputDefinitions));
            }

            ChannelLayout = $"{channels}c";

            _outputDefinitions = outputDefinitions;
        }

        public string Key { get; } = "pan";

        public string Value =>
            string.Join("|", Enumerable.Empty<string>().Append(ChannelLayout).Concat(_outputDefinitions));
    }
}
