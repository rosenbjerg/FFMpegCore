namespace FFMpegCore.Arguments;

/// <summary>
///     overlay. Only meaningful inside a complex filter graph, which is where a second input can reach it.
/// </summary>
public class OverlayArgument : IVideoFilterArgument
{
    private readonly Dictionary<string, string> _arguments = new();

    /// <param name="x">Horizontal position, as an ffmpeg expression. <c>W</c>/<c>H</c> are the main input's size, <c>w</c>/<c>h</c> the overlay's.</param>
    /// <param name="y">Vertical position, as an ffmpeg expression.</param>
    /// <param name="eofAction">What to do when the overlay ends: repeat, endall or pass.</param>
    /// <param name="shortest">End the output when the shortest input ends.</param>
    public OverlayArgument(string x = "0", string y = "0", OverlayEofAction? eofAction = null, bool shortest = false)
    {
        _arguments.Add("x", x);
        _arguments.Add("y", y);
        if (eofAction != null)
        {
            _arguments.Add("eof_action", eofAction.Value.Value);
        }

        if (shortest)
        {
            _arguments.Add("shortest", "1");
        }
    }

    public string Key { get; } = "overlay";
    public string Value => string.Join(":", _arguments.Select(pair => $"{pair.Key}={pair.Value}"));
}

public readonly struct OverlayEofAction
{
    private OverlayEofAction(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static OverlayEofAction Repeat => new("repeat");
    public static OverlayEofAction EndAll => new("endall");
    public static OverlayEofAction Pass => new("pass");

    public static implicit operator OverlayEofAction(string value)
    {
        return new OverlayEofAction(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
