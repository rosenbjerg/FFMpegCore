namespace FFMpegCore.Enums;

public readonly struct TeeOnFail
{
    private TeeOnFail(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static TeeOnFail Abort => new("abort");
    public static TeeOnFail Ignore => new("ignore");

    public static implicit operator TeeOnFail(string value)
    {
        return new TeeOnFail(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
