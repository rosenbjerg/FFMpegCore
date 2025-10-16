namespace FFMpegCore.Enums;

public static class FileExtension
{
    public static readonly string Mp4 = VideoType.Mp4.Extension;
    public static readonly string Ts = VideoType.MpegTs.Extension;
    public static readonly string Ogv = VideoType.Ogv.Extension;
    public static readonly string WebM = VideoType.WebM.Extension;
    public static readonly string Mp3 = ".mp3";
    public static readonly string Gif = ".gif";

    public static string Extension(this Codec type)
    {
        return type.Name switch
        {
            "libx264" => Mp4,
            "libxvpx" => WebM,
            "libxtheora" => Ogv,
            "mpegts" => Ts,
            "png" => Image.Png,
            "jpg" => Image.Jpg,
            "bmp" => Image.Bmp,
            "webp" => Image.Webp,
            _ => throw new Exception("The extension for this video type is not defined.")
        };
    }

    public static class Image
    {
        public const string Png = ".png";
        public const string Jpg = ".jpg";
        public const string Bmp = ".bmp";
        public const string Webp = ".webp";
        public static readonly List<string> All = [Png, Jpg, Bmp, Webp];
    }
}
