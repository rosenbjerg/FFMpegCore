namespace FFMpegCore.Enums;

public readonly struct AudioGateMode
{
    private AudioGateMode(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static AudioGateMode Downward => new("downward");
    public static AudioGateMode Upward => new("upward");

    public static implicit operator AudioGateMode(string value)
    {
        return new AudioGateMode(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
