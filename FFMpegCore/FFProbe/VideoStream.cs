using FFMpegCore.Enums;

namespace FFMpegCore
{
    public class VideoStream : MediaStream
    {
        public double AvgFrameRate { get; set; }
        public int BitsPerRawSample { get; set; }
        public (int Width, int Height) DisplayAspectRatio { get; set; }
        public (int Width, int Height) SampleAspectRatio { get; set; }
        public string Profile { get; set; } = null!;
        public int Width { get; set; }
        public int Height { get; set; }
        public double FrameRate { get; set; }
        public string PixelFormat { get; set; } = null!;
        public int Rotation { get; set; }
        public double AverageFrameRate { get; set; }
        public string ColorRange { get; set; } = null!;
        public string ColorSpace { get; set; } = null!;
        public string ColorTransfer { get; set; } = null!;
        public string ColorPrimaries { get; set; } = null!;

        public PixelFormat GetPixelFormatInfo() => FFMpeg.GetPixelFormat(PixelFormat);
    }
}
