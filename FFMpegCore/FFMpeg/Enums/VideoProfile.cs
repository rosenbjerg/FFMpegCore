namespace FFMpegCore.Enums;

public readonly struct VideoProfile
{
    private VideoProfile(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static VideoProfile Baseline => new("baseline");
    public static VideoProfile Main => new("main");
    public static VideoProfile High => new("high");
    public static VideoProfile High10 => new("high10");
    public static VideoProfile High422 => new("high422");
    public static VideoProfile High444 => new("high444");
    public static VideoProfile Main10 => new("main10");

    public static implicit operator VideoProfile(string value)
    {
        return new VideoProfile(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
