using System;

namespace FFMpegCore
{
    public class MediaFormat
    {
        public TimeSpan Duration { get; set; }
        public string FormatName { get; set; } = null!;
        public string FormatLongName { get; set; } = null!;
        public int StreamCount { get; set; }
        public double ProbeScore { get; set; }
        public double BitRate { get; set; }
    }
}