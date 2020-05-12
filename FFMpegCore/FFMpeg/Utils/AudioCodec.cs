using FFMpegCore.Models;

namespace FFMpegCore.Utils
{
    public static class AudioCodec
    {
        public static Codec Aac => FFMpegUtils.GetCodec("aac");
        public static Codec LibVorbis => FFMpegUtils.GetCodec("libvorbis");
        public static Codec LibFdkAac => FFMpegUtils.GetCodec("libfdk_aac");
        public static Codec Ac3 => FFMpegUtils.GetCodec("ac3");
        public static Codec Eac3 => FFMpegUtils.GetCodec("eac3");
        public static Codec LibMp3Lame => FFMpegUtils.GetCodec("libmp3lame");        
    }
}