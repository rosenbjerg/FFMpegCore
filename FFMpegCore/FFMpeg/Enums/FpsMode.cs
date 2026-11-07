namespace FFMpegCore.Enums;

public readonly struct FpsMode
{
    private FpsMode(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static FpsMode Auto => new("auto");
    public static FpsMode ConstantFrameRate => new("cfr");
    public static FpsMode VariableFrameRate => new("vfr");
    public static FpsMode Passthrough => new("passthrough");

    public static implicit operator FpsMode(string value)
    {
        return new FpsMode(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
