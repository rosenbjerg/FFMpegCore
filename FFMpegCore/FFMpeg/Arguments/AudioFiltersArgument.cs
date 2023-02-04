using FFMpegCore.Exceptions;

namespace FFMpegCore.Arguments
{
    public class AudioFiltersArgument : IArgument
    {
        public readonly AudioFilterOptions Options;

        public AudioFiltersArgument(AudioFilterOptions options)
        {
            Options = options;
        }

        public string Text => GetText();

        private string GetText()
        {
            if (!Options.Arguments.Any())
            {
                throw new FFMpegArgumentException("No audio-filter arguments provided");
            }

            var arguments = Options.Arguments
                .Where(arg => !string.IsNullOrEmpty(arg.Value))
                .Select(arg =>
                {
                    var escapedValue = arg.Value.Replace(",", "\\,");
                    return string.IsNullOrEmpty(arg.Key) ? escapedValue : $"{arg.Key}={escapedValue}";
                });

            return $"-af \"{string.Join(", ", arguments)}\"";
        }
    }

    public interface IAudioFilterArgument
    {
        string Key { get; }
        string Value { get; }
    }

    public class AudioFilterOptions
    {
        public List<IAudioFilterArgument> Arguments { get; } = new();

        public AudioFilterOptions Pan(string channelLayout, params string[] outputDefinitions) => WithArgument(new PanArgument(channelLayout, outputDefinitions));
        public AudioFilterOptions Pan(int channels, params string[] outputDefinitions) => WithArgument(new PanArgument(channels, outputDefinitions));
        public AudioFilterOptions DynamicNormalizer(int frameLength = 500, int filterWindow = 31, double targetPeak = 0.95,
            double gainFactor = 10.0, double targetRms = 0.0, bool channelCoupling = true,
            bool enableDcBiasCorrection = false, bool enableAlternativeBoundary = false,
            double compressorFactor = 0.0) => WithArgument(new DynamicNormalizerArgument(frameLength, filterWindow,
            targetPeak, gainFactor, targetRms, channelCoupling, enableDcBiasCorrection, enableAlternativeBoundary,
            compressorFactor));
        public AudioFilterOptions HighPass(double frequency = 3000, int poles = 2, string width_type = "q", double width = 0.707,
            double mix = 1, string channels = "", bool normalize = false, string transform = "", string precision = "auto",
            int? blocksize = null) => WithArgument(new HighPassFilterArgument(frequency, poles, width_type, width, mix, channels, normalize, transform, precision, blocksize));
        public AudioFilterOptions LowPass(double frequency = 3000, int poles = 2, string width_type = "q", double width = 0.707,
            double mix = 1, string channels = "", bool normalize = false, string transform = "", string precision = "auto",
            int? blocksize = null) => WithArgument(new LowPassFilterArgument(frequency, poles, width_type, width, mix, channels, normalize, transform, precision, blocksize));
        public AudioFilterOptions AudioGate(double level_in = 1, string mode = "downward", double range = 0.06125, double threshold = 0.125,
            int ratio = 2, double attack = 20, double release = 250, int makeup = 1, double knee = 2.828427125, string detection = "rms",
            string link = "average") => WithArgument(new AudioGateArgument(level_in, mode, range, threshold, ratio, attack, release, makeup, knee, detection, link));
        public AudioFilterOptions SilenceDetect(string noise_type = "db", double noise = 60, double duration = 2,
            bool mono = false) => WithArgument(new SilenceDetectArgument(noise_type, noise, duration, mono));

        private AudioFilterOptions WithArgument(IAudioFilterArgument argument)
        {
            Arguments.Add(argument);
            return this;
        }
    }
}
