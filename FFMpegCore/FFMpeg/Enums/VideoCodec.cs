namespace FFMpegCore.Enums;

public static class VideoCodec
{
    public static Codec LibX264 => new("libx264", CodecType.Video);
    public static Codec LibX265 => new("libx265", CodecType.Video);
    public static Codec LibVpx => new("libvpx", CodecType.Video);
    public static Codec LibTheora => new("libtheora", CodecType.Video);
    public static Codec MpegTs => new("mpegts", CodecType.Video);
    public static Codec LibaomAv1 => new("libaom-av1", CodecType.Video);

    public static class Image
    {
        public static Codec Png => new("png", CodecType.Video);
        public static Codec Jpg => new("mjpeg", CodecType.Video);
        public static Codec Bmp => new("bmp", CodecType.Video);
        public static Codec Webp => new("webp", CodecType.Video);

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
