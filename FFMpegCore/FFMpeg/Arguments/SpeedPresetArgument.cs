using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents speed parameter
    /// </summary>
    public class SpeedPresetArgument : IArgument
    {
        public readonly Speed Speed;

        public SpeedPresetArgument(Speed speed)
        {
            Speed = speed;
        }

        public string Text => $"-preset {Speed.ToString().ToLower()}";
    }
}
