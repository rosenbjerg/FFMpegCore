namespace FFMpegCore.Enums;

public static class SubtitleCodec
{
    public static Codec MovText => new("mov_text", CodecType.Subtitle);
    public static Codec Srt => new("srt", CodecType.Subtitle);
    public static Codec Ass => new("ass", CodecType.Subtitle);
    public static Codec WebVtt => new("webvtt", CodecType.Subtitle);
    public static Codec Copy => new("copy", CodecType.Subtitle);
}
