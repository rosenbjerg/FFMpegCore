namespace FFMpegCore.Arguments;

/// <summary>
///     Maps a label a complex-filter chain produced, rather than a stream of an input file.
/// </summary>
public class MapLabelArgument : IArgument
{
    private readonly string _label;
    private readonly bool _negativeMap;

    public MapLabelArgument(string label, bool negativeMap = false)
    {
        _label = label.Trim('[', ']');
        _negativeMap = negativeMap;
    }

    public string Text => $"-map {(_negativeMap ? "-" : "")}\"[{_label}]\"";
}
