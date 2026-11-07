namespace FFMpegCore.Enums;

public readonly struct FilterPrecision
{
    private FilterPrecision(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static FilterPrecision Auto => new("auto");
    public static FilterPrecision S16 => new("s16");
    public static FilterPrecision S32 => new("s32");
    public static FilterPrecision F32 => new("f32");
    public static FilterPrecision F64 => new("f64");

    public static implicit operator FilterPrecision(string value)
    {
        return new FilterPrecision(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
