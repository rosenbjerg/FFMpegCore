namespace FFMpegCore.Enums;

public readonly struct EncoderPreset
{
    private EncoderPreset(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static EncoderPreset UltraFast => new("ultrafast");
    public static EncoderPreset SuperFast => new("superfast");
    public static EncoderPreset VeryFast => new("veryfast");
    public static EncoderPreset Faster => new("faster");
    public static EncoderPreset Fast => new("fast");
    public static EncoderPreset Medium => new("medium");
    public static EncoderPreset Slow => new("slow");
    public static EncoderPreset Slower => new("slower");
    public static EncoderPreset VerySlow => new("veryslow");
    public static EncoderPreset Placebo => new("placebo");

    public static implicit operator EncoderPreset(string value)
    {
        return new EncoderPreset(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
