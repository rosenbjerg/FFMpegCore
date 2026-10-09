using FFMpegCore.Enums;
using FFMpegCore.Helpers;

namespace FFMpegCore.Arguments;

public class SilenceDetectArgument : IAudioFilterArgument
{
    private readonly Dictionary<string, string> _arguments = new();

    /// <summary>
    ///     Silence Detection. <see href="https://ffmpeg.org/ffmpeg-filters.html#silencedetect" />
    /// </summary>
    /// <param name="noiseType">Set noise type to db (decibel) or ar (amplitude ratio). Default is dB</param>
    /// <param name="noise">
    ///     Set noise tolerance. Can be specified in dB (in case "dB" is appended to the specified value) or amplitude ratio.
    ///     Default is -60dB, or 0.001.
    /// </param>
    /// <param name="duration">
    ///     Set silence duration until notification (default is 2 seconds). See (ffmpeg-utils)the Time duration section in the
    ///     ffmpeg-utils(1) manual for the accepted syntax.
    /// </param>
    /// <param name="mono">Process each channel separately, instead of combined. By default is disabled.</param>
    public SilenceDetectArgument(SilenceDetectNoiseUnit? noiseType = null, double noise = -60, double duration = 2, bool mono = false)
    {
        var unit = (noiseType ?? SilenceDetectNoiseUnit.Decibels).Value;
        if (unit == SilenceDetectNoiseUnit.Decibels.Value)
        {
            _arguments.Add("n", $"{noise.ToInvariantString()}dB");
        }
        else if (unit == SilenceDetectNoiseUnit.AmplitudeRatio.Value)
        {
            _arguments.Add("n", noise.ToInvariantString());
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(noiseType), "Noise type must be either db or ar");
        }

        _arguments.Add("d", duration.ToInvariantString());
        _arguments.Add("m", (mono ? 1 : 0).ToString());
    }

    public string Key { get; } = "silencedetect";

    public string Value => string.Join(":", _arguments.Select(pair => $"{pair.Key}={pair.Value}"));
}
