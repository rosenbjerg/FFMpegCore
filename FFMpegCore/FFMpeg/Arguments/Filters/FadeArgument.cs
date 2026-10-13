using System.Globalization;
using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

/// <summary>fade</summary>
public class VideoFadeArgument : IVideoFilterArgument
{
    private readonly Dictionary<string, string> _arguments = new();

    /// <param name="direction">Whether the picture fades in or out.</param>
    /// <param name="start">Where the fade begins.</param>
    /// <param name="duration">How long the fade lasts.</param>
    /// <param name="color">Colour to fade from or to.</param>
    public VideoFadeArgument(FadeDirection direction, TimeSpan start, TimeSpan duration, string? color = null)
    {
        _arguments.Add("t", direction == FadeDirection.In ? "in" : "out");
        _arguments.Add("st", start.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture));
        _arguments.Add("d", duration.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture));
        if (color != null)
        {
            _arguments.Add("c", color);
        }
    }

    public string Key { get; } = "fade";
    public string Value => string.Join(":", _arguments.Select(pair => $"{pair.Key}={pair.Value}"));
}

/// <summary>afade</summary>
public class AudioFadeArgument : IAudioFilterArgument
{
    private readonly Dictionary<string, string> _arguments = new();

    /// <param name="direction">Whether the sound fades in or out.</param>
    /// <param name="start">Where the fade begins.</param>
    /// <param name="duration">How long the fade lasts.</param>
    /// <param name="curve">Shape of the fade; ffmpeg's default is <see cref="FadeCurve.Triangular" />.</param>
    public AudioFadeArgument(FadeDirection direction, TimeSpan start, TimeSpan duration, FadeCurve? curve = null)
    {
        _arguments.Add("t", direction == FadeDirection.In ? "in" : "out");
        _arguments.Add("st", start.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture));
        _arguments.Add("d", duration.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture));
        if (curve != null)
        {
            _arguments.Add("curve", curve.Value.Value);
        }
    }

    public string Key { get; } = "afade";
    public string Value => string.Join(":", _arguments.Select(pair => $"{pair.Key}={pair.Value}"));
}
