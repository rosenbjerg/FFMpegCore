namespace FFMpegCore.Enums;

public static class AudioCodec
{
    public static Codec Aac => FFMpeg.GetCodec("aac");
    public static Codec LibVorbis => FFMpeg.GetCodec("libvorbis");
    public static Codec LibFdk_Aac => FFMpeg.GetCodec("libfdk_aac");
    public static Codec Ac3 => FFMpeg.GetCodec("ac3");
    public static Codec Eac3 => FFMpeg.GetCodec("eac3");
    public static Codec LibMp3Lame => FFMpeg.GetCodec("libmp3lame");
    public static Codec Copy => new("copy", CodecType.Audio);
}
