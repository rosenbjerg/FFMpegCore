using System.Collections.Generic;

namespace FFMpegCore.FFMPEG.Argument
{

    /// <summary>
    /// Represents parameter of concat argument
    /// Used for creating video from multiple images or videos
    /// </summary>
    public class ConcatArgument : IInputArgument
    {
        public readonly IEnumerable<string> Values;
        public ConcatArgument(IEnumerable<string> values)
        {
            Values = values;
        }

        public string Text => $"-i \"concat:{string.Join(@"|", Values)}\"";
    }
}
