namespace FFMpegCore.Enums;

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
