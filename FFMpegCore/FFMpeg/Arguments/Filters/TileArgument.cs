namespace FFMpegCore.Arguments;

/// <summary>tile</summary>
public class TileArgument : IVideoFilterArgument
{
    private readonly Dictionary<string, string> _arguments = new();

    /// <param name="columns">Tiles across.</param>
    /// <param name="rows">Tiles down.</param>
    /// <param name="margin">Outer margin in pixels.</param>
    /// <param name="padding">Space between tiles in pixels.</param>
    /// <param name="color">Background colour behind the tiles.</param>
    public TileArgument(int columns, int rows, int margin = 0, int padding = 0, string? color = null)
    {
        if (columns < 1 || rows < 1)
        {
            throw new ArgumentOutOfRangeException(columns < 1 ? nameof(columns) : nameof(rows), "Tile layout must be at least one by one");
        }

        _arguments.Add("layout", $"{columns}x{rows}");
        if (margin > 0)
        {
            _arguments.Add("margin", margin.ToString());
        }

        if (padding > 0)
        {
            _arguments.Add("padding", padding.ToString());
        }

        if (color != null)
        {
            _arguments.Add("color", color);
        }
    }

    public string Key { get; } = "tile";
    public string Value => string.Join(":", _arguments.Select(pair => $"{pair.Key}={pair.Value}"));
}
