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
    public OverlayArgument(string x = "0", string y = "0", string? eofAction = null, bool shortest = false)
    {
        _arguments.Add("x", x);
        _arguments.Add("y", y);
        if (eofAction != null)
        {
            _arguments.Add("eof_action", eofAction);
        }

        if (shortest)
        {
            _arguments.Add("shortest", "1");
        }
    }

    public string Key { get; } = "overlay";
    public string Value => string.Join(":", _arguments.Select(pair => $"{pair.Key}={pair.Value}"));
}
