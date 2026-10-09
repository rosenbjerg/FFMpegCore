namespace FFMpegCore.Enums;

public readonly struct SilenceDetectNoiseUnit
{
    private SilenceDetectNoiseUnit(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static SilenceDetectNoiseUnit Decibels => new("db");
    public static SilenceDetectNoiseUnit AmplitudeRatio => new("ar");

    public static implicit operator SilenceDetectNoiseUnit(string value)
    {
        return new SilenceDetectNoiseUnit(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
