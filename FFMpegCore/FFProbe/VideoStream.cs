using FFMpegCore.Enums;

namespace FFMpegCore
{
    public class VideoStream : MediaStream
    {
        public double AvgFrameRate { get; set; }
        public int BitsPerRawSample { get; set; }
        public (int Width, int Height) DisplayAspectRatio { get; set; }
        public string Profile { get; set; } = null!;
        public int Width { get; set; }
        public int Height { get; set; }
        public double FrameRate { get; set; }
        public string PixelFormat { get; set; } = null!;
        public int Rotation { get; set; }
        public double AverageFrameRate { get; set; }

        public PixelFormat GetPixelFormatInfo() => FFMpeg.GetPixelFormat(PixelFormat);
    }
}