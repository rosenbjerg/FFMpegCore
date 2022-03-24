using FFMpegCore.Enums;

namespace FFMpegCore
{
    public class VideoStream : MediaStream
    {
        public double AvgFrameRate { get; internal set; }
        public int BitsPerRawSample { get; internal set; }
        public (int Width, int Height) DisplayAspectRatio { get; internal set; }
        public string Profile { get; internal set; } = null!;
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public double FrameRate { get; internal set; }
        public string PixelFormat { get; internal set; } = null!;
        public int Rotation { get; set; }
        public double AverageFrameRate { get; set; }

        public PixelFormat GetPixelFormatInfo() => FFMpeg.GetPixelFormat(PixelFormat);
    }
}