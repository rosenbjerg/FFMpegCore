using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FFMpegCore.Arguments
{
     public class AudioGateArgument : IAudioFilterArgument
    {
        private readonly Dictionary<string, string> _arguments = new Dictionary<string, string>();

        /// <summary>
        ///     Audio Gate. <see href="https://ffmpeg.org/ffmpeg-filters.html#agate"/>
        /// </summary>
        /// <param name="level_in">Set input level before filtering. Default is 1. Allowed range is from 0.015625 to 64.</param>
        /// <param name="mode">Set the mode of operation. Can be upward or downward. Default is downward. If set to upward mode, higher parts of signal will be amplified, expanding dynamic range in upward direction. Otherwise, in case of downward lower parts of signal will be reduced.</param>
        /// <param name="range">Set the level of gain reduction when the signal is below the threshold. Default is 0.06125. Allowed range is from 0 to 1. Setting this to 0 disables reduction and then filter behaves like expander.</param>
        /// <param name="threshold">If a signal rises above this level the gain reduction is released. Default is 0.125. Allowed range is from 0 to 1.</param>
        /// <param name="ratio">Set a ratio by which the signal is reduced. Default is 2. Allowed range is from 1 to 9000.</param>
        /// <param name="attack">Amount of milliseconds the signal has to rise above the threshold before gain reduction stops. Default is 20 milliseconds. Allowed range is from 0.01 to 9000.</param>
        /// <param name="release">Amount of milliseconds the signal has to fall below the threshold before the reduction is increased again. Default is 250 milliseconds. Allowed range is from 0.01 to 9000.</param>
        /// <param name="makeup">Set amount of amplification of signal after processing. Default is 1. Allowed range is from 1 to 64.</param>
        /// <param name="knee">Curve the sharp knee around the threshold to enter gain reduction more softly. Default is 2.828427125. Allowed range is from 1 to 8.</param>
        /// <param name="detection">Choose if exact signal should be taken for detection or an RMS like one. Default is rms. Can be peak or rms.</param>
        /// <param name="link">Choose if the average level between all channels or the louder channel affects the reduction. Default is average. Can be average or maximum.</param>
        public AudioGateArgument(double level_in = 1, string mode = "downward", double range = 0.06125, double threshold = 0.125, int ratio = 2, double attack = 20, double release = 250, int makeup = 1, double knee = 2.828427125, string detection = "rms", string link = "average")
        {
            if (level_in < 0.015625 || level_in > 64) throw new ArgumentOutOfRangeException(nameof(level_in), "Level in must be between 0.015625 to 64");
            if (!(mode == "upward" || mode == "downward")) throw new ArgumentOutOfRangeException(nameof(mode), "Mode must be either upward or downward");
            if (range <= 0 || range > 1) throw new ArgumentOutOfRangeException(nameof(range));
            if (threshold < 0 || threshold > 1) throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be between 0 and 1");
            if (ratio < 1 || ratio > 9000) throw new ArgumentOutOfRangeException(nameof(ratio), "Ratio must be between 1 and 9000");
            if (attack < 0.01 || attack > 9000) throw new ArgumentOutOfRangeException(nameof(attack), "Attack must be between 0.01 and 9000");
            if (release < 0.01 || release > 9000) throw new ArgumentOutOfRangeException(nameof(release), "Release must be between 0.01 and 9000");
            if (makeup < 1 || makeup > 64) throw new ArgumentOutOfRangeException(nameof(makeup), "Makeup Gain must be between 1 and 64");
            if (!(detection == "peak" || detection == "rms")) throw new ArgumentOutOfRangeException(nameof(detection), "Detection must be either peak or rms");
            if (!(link != "average" || link != "maximum")) throw new ArgumentOutOfRangeException(nameof(link), "Link must be either average or maximum");

            _arguments.Add("level_in", level_in.ToString("0.00", CultureInfo.InvariantCulture));
            _arguments.Add("mode", mode.ToString());
            _arguments.Add("range", range.ToString("0.00", CultureInfo.InvariantCulture));
            _arguments.Add("threshold", threshold.ToString("0.00", CultureInfo.InvariantCulture));
            _arguments.Add("ratio", ratio.ToString());
            _arguments.Add("attack", attack.ToString("0.00", CultureInfo.InvariantCulture));
            _arguments.Add("release", release.ToString("0.00", CultureInfo.InvariantCulture));
            _arguments.Add("makeup", makeup.ToString());
            _arguments.Add("detection", detection.ToString());
            _arguments.Add("link", link.ToString());
        }

        public string Key { get; } = "agate";

        public string Value => string.Join(":", _arguments.Select(pair => $"{pair.Key}={pair.Value}"));
    }
}
