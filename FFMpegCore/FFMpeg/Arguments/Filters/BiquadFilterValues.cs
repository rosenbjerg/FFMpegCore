namespace FFMpegCore.Arguments;

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

public readonly struct FilterTransform
{
    private FilterTransform(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static FilterTransform DirectFormI => new("di");
    public static FilterTransform DirectFormII => new("dii");
    public static FilterTransform TransposedDirectFormI => new("tdi");
    public static FilterTransform TransposedDirectFormII => new("tdii");
    public static FilterTransform Lattice => new("latt");
    public static FilterTransform StateVariable => new("svf");
    public static FilterTransform ZeroDelay => new("zdf");

    public static implicit operator FilterTransform(string value)
    {
        return new FilterTransform(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
