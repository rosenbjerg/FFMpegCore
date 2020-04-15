namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Audio sampling rate argument. Defaults to 48000 (Hz)
    /// </summary>
    public class AudioSamplingRateArgument : Argument<int>
    {
        public AudioSamplingRateArgument() : base(48000) { }

        public AudioSamplingRateArgument(int samplingRate) : base(samplingRate) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"-ar {Value}";
        }
    }
}