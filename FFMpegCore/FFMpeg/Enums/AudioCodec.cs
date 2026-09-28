namespace FFMpegCore.Enums;

public static class AudioCodec
{
    public static Codec Aac => new("aac", CodecType.Audio);
    public static Codec LibVorbis => new("libvorbis", CodecType.Audio);
    public static Codec LibFdkAac => new("libfdk_aac", CodecType.Audio);
    public static Codec Ac3 => new("ac3", CodecType.Audio);
    public static Codec Eac3 => new("eac3", CodecType.Audio);
    public static Codec LibMp3Lame => new("libmp3lame", CodecType.Audio);
    public static Codec Copy => new("copy", CodecType.Audio);
}
