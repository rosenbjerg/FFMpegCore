using System.Collections;
using System.Collections.Generic;

namespace FFMpegCore.FFMPEG.Argument
{

    /// <summary>
    /// Represents parameter of concat argument
    /// Used for creating video from multiple images or videos
    /// </summary>
    public class ConcatArgument : Argument<IEnumerable<string>>, IEnumerable<string>
    {
        public ConcatArgument() : base(new List<string>()) { }

        public ConcatArgument(IEnumerable<string> value) : base(value) { }

        public IEnumerator<string> GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"-i \"concat:{string.Join(@"|", Value)}\"";
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
