namespace FFMpegCore.Enums;

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
