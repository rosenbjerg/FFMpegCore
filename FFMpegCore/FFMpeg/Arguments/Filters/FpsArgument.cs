using System.Globalization;

namespace FFMpegCore.Arguments;

/// <summary>fps</summary>
public class FpsArgument : IVideoFilterArgument
{
    private readonly Dictionary<string, string> _arguments = new();

    /// <param name="frameRate">Frames per second to output, duplicating or dropping frames to reach it.</param>
    /// <param name="round">Rounding for the timestamps: zero, inf, down, up or near.</param>
    public FpsArgument(double frameRate, FpsRounding? round = null)
    {
        if (frameRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frameRate), "Frame rate must be a positive number");
        }

        _arguments.Add("fps", frameRate.ToString("0.####", CultureInfo.InvariantCulture));
        if (round != null)
        {
            _arguments.Add("round", round.Value.Value);
        }
    }

    public string Key { get; } = "fps";
    public string Value => string.Join(":", _arguments.Select(pair => $"{pair.Key}={pair.Value}"));
}

public readonly struct FpsRounding
{
    private FpsRounding(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static FpsRounding Zero => new("zero");
    public static FpsRounding Infinity => new("inf");
    public static FpsRounding Down => new("down");
    public static FpsRounding Up => new("up");
    public static FpsRounding Near => new("near");

    public static implicit operator FpsRounding(string value)
    {
        return new FpsRounding(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
