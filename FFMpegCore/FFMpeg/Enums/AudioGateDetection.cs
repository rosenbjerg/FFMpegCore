namespace FFMpegCore.Enums;

public readonly struct AudioGateDetection
{
    private AudioGateDetection(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static AudioGateDetection Peak => new("peak");
    public static AudioGateDetection Rms => new("rms");

    public static implicit operator AudioGateDetection(string value)
    {
        return new AudioGateDetection(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
