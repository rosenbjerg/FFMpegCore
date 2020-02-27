using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents cpu speed parameter
    /// </summary>
    public class DisableChannelArgument : Argument<Channel>
    {
        public DisableChannelArgument() { }

        public DisableChannelArgument(Channel value) : base(value) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return Value switch
            {
                Channel.Video => "-vn",
                Channel.Audio => "-an",
                _ => string.Empty
            };
        }
    }
}
