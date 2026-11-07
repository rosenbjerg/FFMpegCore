namespace FFMpegCore.Enums;

public readonly struct StreamDisposition
{
    private StreamDisposition(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static StreamDisposition None => new("0");
    public static StreamDisposition Default => new("default");
    public static StreamDisposition Dub => new("dub");
    public static StreamDisposition Original => new("original");
    public static StreamDisposition Comment => new("comment");
    public static StreamDisposition Lyrics => new("lyrics");
    public static StreamDisposition Karaoke => new("karaoke");
    public static StreamDisposition Forced => new("forced");
    public static StreamDisposition HearingImpaired => new("hearing_impaired");
    public static StreamDisposition VisualImpaired => new("visual_impaired");
    public static StreamDisposition CleanEffects => new("clean_effects");
    public static StreamDisposition AttachedPicture => new("attached_pic");
    public static StreamDisposition Captions => new("captions");
    public static StreamDisposition Descriptions => new("descriptions");
    public static StreamDisposition Metadata => new("metadata");
    public static StreamDisposition Dependent => new("dependent");
    public static StreamDisposition StillImage => new("still_image");

    public static StreamDisposition operator +(StreamDisposition left, StreamDisposition right)
    {
        return new StreamDisposition($"{left.Value}+{right.Value}");
    }

    public static implicit operator StreamDisposition(string value)
    {
        return new StreamDisposition(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
