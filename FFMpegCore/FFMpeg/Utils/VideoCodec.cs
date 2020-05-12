using FFMpegCore.Models;

namespace FFMpegCore.Utils
{
    public static class VideoCodec
    {
        public static Codec LibX264 => FFMpegUtils.GetCodec("libx264");
        public static Codec LibVpx => FFMpegUtils.GetCodec("libvpx");
        public static Codec LibTheora => FFMpegUtils.GetCodec("libtheora");
        public static Codec Png => FFMpegUtils.GetCodec("png");
        public static Codec MpegTs => FFMpegUtils.GetCodec("mpegts");
    }
}