using System;
using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents parameter of bitstream filter
    /// </summary>
    public class BitStreamFilterArgument : Argument<Channel, Filter>
    {
        public BitStreamFilterArgument() { }

        public BitStreamFilterArgument(Channel first, Filter second) : base(first, second) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return First switch
            {
                Channel.Audio => $"-bsf:a {Second.ToString().ToLower()}",
                Channel.Video => $"-bsf:v {Second.ToString().ToLower()}",
                _ => string.Empty
            };
        }
    }
}
