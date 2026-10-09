namespace FFMpegCore.Enums;

public readonly struct MovFlags
{
    private MovFlags(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static MovFlags FastStart => new("faststart");
    public static MovFlags FragmentKeyframe => new("frag_keyframe");
    public static MovFlags FragmentEveryFrame => new("frag_every_frame");
    public static MovFlags EmptyMoov => new("empty_moov");
    public static MovFlags SeparateMoof => new("separate_moof");
    public static MovFlags DefaultBaseMoof => new("default_base_moof");
    public static MovFlags Cmaf => new("cmaf");
    public static MovFlags UseMetadataTags => new("use_metadata_tags");

    public static MovFlags operator +(MovFlags left, MovFlags right)
    {
        return new MovFlags($"{left.Value}+{right.Value}");
    }

    public static implicit operator MovFlags(string value)
    {
        return new MovFlags(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
