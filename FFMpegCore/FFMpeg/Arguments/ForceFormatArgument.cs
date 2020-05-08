using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents force format parameter
    /// </summary>
    public class ForceFormatArgument : IArgument
    {
        private readonly string _format;
        public ForceFormatArgument(string format)
        {
            _format = format;
        }

        public ForceFormatArgument(VideoCodec value) : this(value.ToString().ToLower()) { }

        public string Text => $"-f {_format}";
    }
}
