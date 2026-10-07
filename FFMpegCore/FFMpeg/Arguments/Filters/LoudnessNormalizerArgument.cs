using System.Globalization;

namespace FFMpegCore.Arguments;

/// <summary>loudnorm, EBU R128 loudness normalisation</summary>
public class LoudnessNormalizerArgument : IAudioFilterArgument
{
    private readonly Dictionary<string, string> _arguments = new();

    /// <param name="integratedLoudness">Target integrated loudness in LUFS, between -70 and -5.</param>
    /// <param name="loudnessRange">Target loudness range in LU, between 1 and 20.</param>
    /// <param name="truePeak">Maximum true peak in dBTP, between -9 and 0.</param>
    /// <param name="dualMono">Treat mono input as dual-mono, matching how it is perceived against stereo.</param>
    public LoudnessNormalizerArgument(double integratedLoudness = -24, double loudnessRange = 7, double truePeak = -2, bool dualMono = false)
    {
        if (integratedLoudness < -70 || integratedLoudness > -5)
        {
            throw new ArgumentOutOfRangeException(nameof(integratedLoudness), "Integrated loudness must be between -70 and -5 LUFS");
        }

        if (loudnessRange < 1 || loudnessRange > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(loudnessRange), "Loudness range must be between 1 and 20 LU");
        }

        if (truePeak < -9 || truePeak > 0)
        {
            throw new ArgumentOutOfRangeException(nameof(truePeak), "True peak must be between -9 and 0 dBTP");
        }

        _arguments.Add("I", integratedLoudness.ToString("0.0", CultureInfo.InvariantCulture));
        _arguments.Add("LRA", loudnessRange.ToString("0.0", CultureInfo.InvariantCulture));
        _arguments.Add("TP", truePeak.ToString("0.0", CultureInfo.InvariantCulture));
        if (dualMono)
        {
            _arguments.Add("dual_mono", "true");
        }
    }

    public string Key { get; } = "loudnorm";
    public string Value => string.Join(":", _arguments.Select(pair => $"{pair.Key}={pair.Value}"));
}
