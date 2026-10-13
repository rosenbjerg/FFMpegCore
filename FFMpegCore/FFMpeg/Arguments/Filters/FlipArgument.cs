namespace FFMpegCore.Arguments;

public class FlipArgument : IVideoFilterArgument
{
    public static readonly FlipArgument Horizontal = new("hflip");
    public static readonly FlipArgument Vertical = new("vflip");

    private FlipArgument(string filter)
    {
        Value = filter;
    }

    public string Key => string.Empty;
    public string Value { get; }
}
