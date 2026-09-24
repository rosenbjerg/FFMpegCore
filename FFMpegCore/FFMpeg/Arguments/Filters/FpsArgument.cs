using System.Globalization;

namespace FFMpegCore.Arguments;

/// <summary>fps</summary>
public class FpsArgument : IVideoFilterArgument
{
    private readonly Dictionary<string, string> _arguments = new();

    /// <param name="frameRate">Frames per second to output, duplicating or dropping frames to reach it.</param>
    /// <param name="round">Rounding for the timestamps: zero, inf, down, up or near.</param>
    public FpsArgument(double frameRate, string? round = null)
    {
        if (frameRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frameRate), "Frame rate must be a positive number");
        }

        _arguments.Add("fps", frameRate.ToString("0.####", CultureInfo.InvariantCulture));
        if (round != null)
        {
            _arguments.Add("round", round);
        }
    }

    public string Key { get; } = "fps";
    public string Value => string.Join(":", _arguments.Select(pair => $"{pair.Key}={pair.Value}"));
}
