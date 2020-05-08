namespace FFMpegCore.FFMPEG
{
    public class VideoStream : MediaStream
    {
        public double AvgFrameRate { get; internal set; }
        public int BitsPerRawSample { get; internal set; }
        public (int width, int height) DisplayAspectRatio { get; internal set; }
        public string Profile { get; internal set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public double FrameRate { get; internal set; }
        public string PixelFormat { get; internal set; }
    }
}