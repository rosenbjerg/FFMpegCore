using System.Globalization;

namespace FFMpegCore.Arguments;

/// <summary>setpts, restamping the video so it plays at a different speed</summary>
public class VideoSpeedArgument : IVideoFilterArgument
{
    private readonly double _multiplier;

    /// <param name="multiplier">Playback speed, where 2 is twice as fast and 0.5 is half.</param>
    public VideoSpeedArgument(double multiplier)
    {
        if (multiplier <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), "Speed multiplier must be a positive number");
        }

        _multiplier = multiplier;
    }

    public string Key { get; } = "setpts";
    public string Value => $"{(1 / _multiplier).ToString("0.####", CultureInfo.InvariantCulture)}*PTS";
}

/// <summary>
///     atempo. One atempo only spans 0.5x to 100x; <see cref="AudioFilterOptions.Speed" /> chains several for anything outside that.
/// </summary>
public class AudioSpeedArgument : IAudioFilterArgument
{
    private readonly double _factor;

    /// <param name="factor">Resampling factor, which ffmpeg accepts between 0.5 and 100.</param>
    public AudioSpeedArgument(double factor)
    {
        if (factor < 0.5 || factor > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(factor), "A single atempo factor must be between 0.5 and 100");
        }

        _factor = factor;
    }

    public string Key { get; } = "atempo";
    public string Value => _factor.ToString("0.####", CultureInfo.InvariantCulture);

    internal static IEnumerable<double> StepsFor(double multiplier)
    {
        if (multiplier <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), "Speed multiplier must be a positive number");
        }

        var remaining = multiplier;
        while (remaining < 0.5)
        {
            yield return 0.5;
            remaining /= 0.5;
        }

        while (remaining > 100)
        {
            yield return 100;
            remaining /= 100;
        }

        yield return remaining;
    }
}
