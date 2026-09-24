namespace FFMpegCore.Enums;

public static class FileExtension
{
    public static readonly string Mp4 = VideoType.Mp4.Extension;
    public static readonly string Ts = VideoType.Ts.Extension;
    public static readonly string Ogv = VideoType.Ogv.Extension;
    public static readonly string WebM = VideoType.WebM.Extension;
    public static readonly string Mp3 = ".mp3";
    public static readonly string Gif = ".gif";

    public static class Image
    {
        public const string Png = ".png";
        public const string Jpg = ".jpg";
        public const string Bmp = ".bmp";
        public const string Webp = ".webp";
        public static readonly List<string> All = [Png, Jpg, Bmp, Webp];
    }
}
