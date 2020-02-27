using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents parameter of copy parameter
    /// Defines if channel (audio, video or both) should be copied to output file
    /// </summary>
    public class CopyArgument : Argument<Channel>
    {
        public CopyArgument() : base(Channel.Both) { }
        public CopyArgument(Channel value = Channel.Both) : base(value) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return Value switch
            {
                Channel.Audio => "-c:a copy",
                Channel.Video => "-c:v copy",
                _ => "-c copy"
            };
        }
    }
}
