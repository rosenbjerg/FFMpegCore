namespace FFMpegCore.Enums
{
    public enum CodecType
    {
        Unknown = 0,
        Video = 1 << 1,
        Audio = 1 << 2,
        Subtitle = 1 << 3,
        Data = 1 << 4,
    }

    public static class VideoCodec
    {
        public static Codec LibX264 => FFMpeg.GetCodec("libx264");
        public static Codec LibX265 => FFMpeg.GetCodec("libx265");
        public static Codec LibVpx => FFMpeg.GetCodec("libvpx");
        public static Codec LibTheora => FFMpeg.GetCodec("libtheora");
        public static Codec Png => FFMpeg.GetCodec("png");
        public static Codec MpegTs => FFMpeg.GetCodec("mpegts");
    }

    public static class AudioCodec
    {
        public static Codec Aac => FFMpeg.GetCodec("aac");
        public static Codec LibVorbis => FFMpeg.GetCodec("libvorbis");
        public static Codec LibFdk_Aac => FFMpeg.GetCodec("libfdk_aac");
        public static Codec Ac3 => FFMpeg.GetCodec("ac3");
        public static Codec Eac3 => FFMpeg.GetCodec("eac3");
        public static Codec LibMp3Lame => FFMpeg.GetCodec("libmp3lame");        
    }

    public static class VideoType
    {
        public static ContainerFormat MpegTs => FFMpeg.GetContainerFormat("mpegts");
        public static ContainerFormat Ts => FFMpeg.GetContainerFormat("mpegts");
        public static ContainerFormat Mp4 => FFMpeg.GetContainerFormat("mp4");
        public static ContainerFormat Mov => FFMpeg.GetContainerFormat("mov");
        public static ContainerFormat Avi => FFMpeg.GetContainerFormat("avi");
        public static ContainerFormat Ogv => FFMpeg.GetContainerFormat("ogv");
        public static ContainerFormat WebM => FFMpeg.GetContainerFormat("webm");
    }

    public enum Filter
    {
        H264_Mp4ToAnnexB,
        Aac_AdtstoAsc
    }

    public enum Channel
    {
        Audio,
        Video,
        Both
    }
}