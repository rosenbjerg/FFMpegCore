using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FFMpegCore.Arguments
{
    public class SilenceDetectArgument : IAudioFilterArgument
    {
        private readonly Dictionary<string, string> _arguments = new Dictionary<string, string>();
        /// <summary>
        ///     Silence Detection. <see href="https://ffmpeg.org/ffmpeg-filters.html#silencedetect"/>
        /// </summary>
        /// <param name="noise_type">Set noise type to db (decibel) or ar (amplitude ratio). Default is dB</param>
        /// <param name="noise">Set noise tolerance. Can be specified in dB (in case "dB" is appended to the specified value) or amplitude ratio. Default is -60dB, or 0.001.</param>
        /// <param name="duration">Set silence duration until notification (default is 2 seconds). See (ffmpeg-utils)the Time duration section in the ffmpeg-utils(1) manual for the accepted syntax.</param>
        /// <param name="mono">Process each channel separately, instead of combined. By default is disabled.</param>

        public SilenceDetectArgument(string noise_type = "db", double noise = 60, double duration = 2, bool mono = false)
        {
            if(noise_type == "db")
            {
                _arguments.Add("n", $"{noise.ToString("0.0", CultureInfo.InvariantCulture)}dB");
            }
            else if (noise_type == "ar") 
            {
                _arguments.Add("n", noise.ToString("0.00", CultureInfo.InvariantCulture));
            }
            else throw new ArgumentOutOfRangeException(nameof(noise_type), "Noise type must be either db or ar");
            _arguments.Add("d", duration.ToString("0.00", CultureInfo.InvariantCulture));
            _arguments.Add("m", (mono ? 1 : 0).ToString());            
        }

        public string Key { get; } = "silencedetect";

        public string Value => string.Join(":", _arguments.Select(pair => $"{pair.Key}={pair.Value}"));
    }
}
