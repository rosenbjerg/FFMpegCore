namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents cpu speed parameter
    /// </summary>
    public class CpuSpeedArgument : Argument<int>
    {
        public CpuSpeedArgument() { }

        public CpuSpeedArgument(int value) : base(value) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"-quality good -cpu-used {Value} -deadline realtime";
        }
    }
}
