namespace FFMpegCore.Enums;

public readonly struct AudioGateLink
{
    private AudioGateLink(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static AudioGateLink Average => new("average");
    public static AudioGateLink Maximum => new("maximum");

    public static implicit operator AudioGateLink(string value)
    {
        return new AudioGateLink(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
