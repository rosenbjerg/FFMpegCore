namespace FFMpegCore.Enums;

public readonly struct BitstreamFilter
{
    private BitstreamFilter(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static BitstreamFilter H264_Mp4ToAnnexB => new("h264_mp4toannexb");
    public static BitstreamFilter Hevc_Mp4ToAnnexB => new("hevc_mp4toannexb");
    public static BitstreamFilter Aac_AdtsToAsc => new("aac_adtstoasc");
    public static BitstreamFilter Mpeg4_UnpackBFrames => new("mpeg4_unpack_bframes");
    public static BitstreamFilter ExtractExtradata => new("extract_extradata");
    public static BitstreamFilter DumpExtra => new("dump_extra");

    public static implicit operator BitstreamFilter(string value)
    {
        return new BitstreamFilter(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
