namespace FFMpegCore.Enums;

public static class AudioCodec
{
    public static Codec Aac => new("aac", CodecType.Audio);
    public static Codec LibVorbis => new("libvorbis", CodecType.Audio);
    public static Codec LibOpus => new("libopus", CodecType.Audio);
    public static Codec LibFdkAac => new("libfdk_aac", CodecType.Audio);
    public static Codec Ac3 => new("ac3", CodecType.Audio);
    public static Codec Eac3 => new("eac3", CodecType.Audio);
    public static Codec LibMp3Lame => new("libmp3lame", CodecType.Audio);
    public static Codec Flac => new("flac", CodecType.Audio);
    public static Codec Alac => new("alac", CodecType.Audio);
    public static Codec PcmS16Le => new("pcm_s16le", CodecType.Audio);
    public static Codec Copy => new("copy", CodecType.Audio);
}
