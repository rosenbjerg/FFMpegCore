using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FFMpegCore.Arguments
{
    public class DynamicNormalizerArgument : IAudioFilterArgument
    {
        private readonly Dictionary<string, string> _arguments = new Dictionary<string, string>();

        /// <summary>
        ///     Dynamic Audio Normalizer. <see href="https://ffmpeg.org/ffmpeg-filters.html#dynaudnorm"/>
        /// </summary>
        /// <param name="frameLength">Set the frame length in milliseconds. Must be between 10 to 8000. The default value is 500</param>
        /// <param name="filterWindow">Set the Gaussian filter window size. In range from 3 to 301, must be odd number. The default value is 31</param>
        /// <param name="targetPeak">Set the target peak value. The default value is 0.95</param>
        /// <param name="gainFactor">Set the maximum gain factor. In range from 1.0 to 100.0. Default is 10.0.</param>
        /// <param name="targetRms">Set the target RMS. In range from 0.0 to 1.0. Default to 0.0 (disabled)</param>
        /// <param name="channelCoupling">Enable channels coupling. By default is enabled.</param>
        /// <param name="enableDcBiasCorrection">Enable DC bias correction. By default is disabled.</param>
        /// <param name="enableAlternativeBoundary">Enable alternative boundary mode. By default is disabled.</param>
        /// <param name="compressorFactor">Set the compress factor. In range from 0.0 to 30.0. Default is 0.0 (disabled).</param>
        public DynamicNormalizerArgument(int frameLength = 500, int filterWindow = 31, double targetPeak = 0.95, double gainFactor = 10.0, double targetRms = 0.0, bool channelCoupling = true, bool enableDcBiasCorrection = false, bool enableAlternativeBoundary = false, double compressorFactor = 0.0)
        {
            if (frameLength < 10 || frameLength > 8000) throw new ArgumentOutOfRangeException(nameof(frameLength),"Frame length must be between 10 to 8000");
            if (filterWindow < 3 || filterWindow > 31) throw new ArgumentOutOfRangeException(nameof(filterWindow), "Gaussian filter window size must be between 3 to 31");
            if (filterWindow % 2 == 0) throw new ArgumentOutOfRangeException(nameof(filterWindow), "Gaussian filter window size must be an odd number");
            if (targetPeak <= 0 || targetPeak > 1) throw new ArgumentOutOfRangeException(nameof(targetPeak));
            if (gainFactor < 1 || gainFactor > 100) throw new ArgumentOutOfRangeException(nameof(gainFactor), "Gain factor must be between 1.0 to 100.0");
            if (targetRms < 0 || targetRms > 1) throw new ArgumentOutOfRangeException(nameof(targetRms), "Target RMS must be between 0.0 and 1.0");
            if (compressorFactor < 0 || compressorFactor > 30) throw new ArgumentOutOfRangeException(nameof(compressorFactor), "Compressor factor must be between 0.0 and 30.0");

            _arguments.Add("f", frameLength.ToString());
            _arguments.Add("g", filterWindow.ToString());
            _arguments.Add("p", targetPeak.ToString("0.00", CultureInfo.InvariantCulture));
            _arguments.Add("m", gainFactor.ToString("0.0", CultureInfo.InvariantCulture));
            _arguments.Add("r", targetRms.ToString("0.0", CultureInfo.InvariantCulture));
            _arguments.Add("n", (channelCoupling ? 1 : 0).ToString());
            _arguments.Add("c", (enableDcBiasCorrection ? 1 : 0).ToString());
            _arguments.Add("b", (enableAlternativeBoundary ? 1 : 0).ToString());
            _arguments.Add("s", compressorFactor.ToString("0.0", CultureInfo.InvariantCulture));
        }

        public string Key { get; } = "dynaudnorm";

        public string Value => string.Join(":", _arguments.Select(pair => $"{pair.Key}={pair.Value}"));
    }
}