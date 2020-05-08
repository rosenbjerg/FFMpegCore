using System;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents duration parameter
    /// </summary>
    public class DurationArgument : IArgument
    {
        public readonly TimeSpan? Duration;
        public DurationArgument(TimeSpan? duration)
        {
            Duration = duration;
        }

        public string Text => !Duration.HasValue ? string.Empty : $"-t {Duration.Value}";
    }
}
