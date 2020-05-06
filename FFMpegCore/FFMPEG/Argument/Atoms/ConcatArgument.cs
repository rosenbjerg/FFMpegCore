using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FFMpegCore.FFMPEG.Argument
{

    /// <summary>
    /// Represents parameter of concat argument
    /// Used for creating video from multiple images or videos
    /// </summary>
    public class ConcatArgument : Argument<IEnumerable<string>>
    {
        public ConcatArgument() : base(new List<string>()) { }

        public ConcatArgument(IEnumerable<string> value) : base(value) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"-i \"concat:{string.Join(@"|", Value)}\"";
        }

        public VideoInfo[] GetAsVideoInfo()
        {
            return Value.Select(v => new VideoInfo(v)).ToArray();
        }
    }
}
