namespace FFMpegCore.Enums;

public static class FileExtension
{
    public const string Mp4 = ".mp4";
    public const string Ts = ".ts";
    public const string Ogv = ".ogv";
    public const string WebM = ".webm";
    public const string Mp3 = ".mp3";
    public const string Gif = ".gif";

    public static class Image
    {
        public const string Png = ".png";
        public const string Jpg = ".jpg";
        public const string Bmp = ".bmp";
        public const string Webp = ".webp";
        public static readonly IReadOnlyList<string> All = [Png, Jpg, Bmp, Webp];
    }
}
