using System.Globalization;

namespace FFMpegCore.Arguments
{
    public class LowPassFilterArgument : IAudioFilterArgument
    {
        private readonly Dictionary<string, string> _arguments = new();
        private readonly List<string> _widthTypes = new() { "h", "q", "o", "s", "k" };
        private readonly List<string> _transformTypes = new() { "di", "dii", "tdi", "tdii", "latt", "svf", "zdf" };
        private readonly List<string> _precision = new() { "auto", "s16", "s32", "f32", "f64" };
        /// <summary>
        ///     LowPass Filter. <see href="https://ffmpeg.org/ffmpeg-filters.html#lowpass"/>
        /// </summary>
        /// <param name="frequency">Set frequency in Hz. Default is 3000.</param>
        /// <param name="poles">Set number of poles. Default is 2.</param>
        /// <param name="width_type">Set method to specify band-width of filter, possible values are: h, q, o, s, k</param>
        /// <param name="width">Specify the band-width of a filter in width_type units. Applies only to double-pole filter. The default is 0.707q and gives a Butterworth response.</param>
        /// <param name="mix">How much to use filtered signal in output. Default is 1. Range is between 0 and 1.</param>
        /// <param name="channels">Specify which channels to filter, by default all available are filtered.</param>
        /// <param name="normalize">Normalize biquad coefficients, by default is disabled. Enabling it will normalize magnitude response at DC to 0dB.</param>
        /// <param name="transform">Set transform type of IIR filter, possible values are: di, dii, tdi, tdii, latt, svf, zdf</param>
        /// <param name="precision">Set precison of filtering, possible values are: auto, s16, s32, f32, f64.</param>
        /// <param name="block_size">Set block size used for reverse IIR processing. If this value is set to high enough value (higher than impulse response length truncated when reaches near zero values) filtering will become linear phase otherwise if not big enough it will just produce nasty artifacts.</param>
        public LowPassFilterArgument(double frequency = 3000, int poles = 2, string width_type = "q", double width = 0.707, double mix = 1, string channels = "", bool normalize = false, string transform = "", string precision = "auto", int? block_size = null)
        {
            if (frequency < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(frequency), "Frequency must be a positive number");
            }

            if (poles < 1 || poles > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(poles), "Poles must be either 1 or 2");
            }

            if (!_widthTypes.Contains(width_type))
            {
                throw new ArgumentOutOfRangeException(nameof(width_type), "Width type must be either " + _widthTypes.ToString());
            }

            if (mix < 0 || mix > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(mix), "Mix must be between 0 and 1");
            }

            if (!_precision.Contains(precision))
            {
                throw new ArgumentOutOfRangeException(nameof(precision), "Precision must be either " + _precision.ToString());
            }

            _arguments.Add("f", frequency.ToString("0.00", CultureInfo.InvariantCulture));
            _arguments.Add("p", poles.ToString());
            _arguments.Add("t", width_type);
            _arguments.Add("w", width.ToString("0.00", CultureInfo.InvariantCulture));
            _arguments.Add("m", mix.ToString("0.00", CultureInfo.InvariantCulture));
            if (channels != "")
            {
                _arguments.Add("c", channels);
            }

            _arguments.Add("n", (normalize ? 1 : 0).ToString());
            if (transform != "" && _transformTypes.Contains(transform))
            {
                _arguments.Add("a", transform);
            }

            _arguments.Add("r", precision);
            if (block_size != null && block_size >= 0)
            {
                _arguments.Add("b", block_size.ToString());
            }
        }

        public string Key { get; } = "lowpass";

        public string Value => string.Join(":", _arguments.Select(pair => $"{pair.Key}={pair.Value}"));
    }
}
