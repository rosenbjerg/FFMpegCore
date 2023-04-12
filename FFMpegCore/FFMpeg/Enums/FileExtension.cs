namespace FFMpegCore.Enums
{
    public static class FileExtension
    {
        public static string Extension(this Codec type)
        {
            return type.Name switch
            {
                "libx264" => Mp4,
                "libxvpx" => WebM,
                "libxtheora" => Ogv,
                "mpegts" => Ts,
                "png" => Png,
                _ => throw new Exception("The extension for this video type is not defined.")
            };
        }
        public static readonly string Mp4 = VideoType.Mp4.Extension;
        public static readonly string Ts = VideoType.MpegTs.Extension;
        public static readonly string Ogv = VideoType.Ogv.Extension;
        public static readonly string WebM = VideoType.WebM.Extension;
        public static readonly string Png = ".png";
        public static readonly string Mp3 = ".mp3";
        public static readonly string Gif = ".gif";
    }
}
