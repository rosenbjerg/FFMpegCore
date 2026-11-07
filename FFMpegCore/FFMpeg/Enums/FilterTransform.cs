namespace FFMpegCore.Enums;

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
