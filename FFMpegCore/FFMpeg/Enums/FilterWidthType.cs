namespace FFMpegCore.Enums;

public readonly struct FilterWidthType
{
    private FilterWidthType(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static FilterWidthType Hertz => new("h");
    public static FilterWidthType QFactor => new("q");
    public static FilterWidthType Octave => new("o");
    public static FilterWidthType Slope => new("s");
    public static FilterWidthType KiloHertz => new("k");

    public static implicit operator FilterWidthType(string value)
    {
        return new FilterWidthType(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
