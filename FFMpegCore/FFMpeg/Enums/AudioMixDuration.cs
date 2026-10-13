namespace FFMpegCore.Enums;

public readonly struct AudioMixDuration
{
    private AudioMixDuration(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static AudioMixDuration Longest => new("longest");
    public static AudioMixDuration Shortest => new("shortest");
    public static AudioMixDuration First => new("first");

    public static implicit operator AudioMixDuration(string value)
    {
        return new AudioMixDuration(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
