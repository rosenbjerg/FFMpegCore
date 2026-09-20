namespace FFMpegCore.Enums;

public static class VideoCodec
{
    public static Codec LibX264 => FFMpeg.GetCodec("libx264");
    public static Codec LibX265 => FFMpeg.GetCodec("libx265");
    public static Codec LibVpx => FFMpeg.GetCodec("libvpx");
    public static Codec LibTheora => FFMpeg.GetCodec("libtheora");
    public static Codec MpegTs => FFMpeg.GetCodec("mpegts");
    public static Codec LibaomAv1 => FFMpeg.GetCodec("libaom-av1");

    public static class Image
    {
        public static Codec Png => FFMpeg.GetCodec("png");
        public static Codec Jpg => FFMpeg.GetCodec("mjpeg");
        public static Codec Bmp => FFMpeg.GetCodec("bmp");
        public static Codec Webp => FFMpeg.GetCodec("webp");

        public static Codec GetByExtension(string path)
        {
            var ext = Path.GetExtension(path);
            switch (ext)
            {
                case FileExtension.Image.Png:
                    return Png;
                case FileExtension.Image.Jpg:
                    return Jpg;
                case FileExtension.Image.Bmp:
                    return Bmp;
                case FileExtension.Image.Webp:
                    return Webp;
                default: throw new NotSupportedException($"Unsupported image extension: {ext}");
            }
        }
    }
}
