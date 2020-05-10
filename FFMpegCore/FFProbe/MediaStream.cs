using System;

namespace FFMpegCore
{
    public class MediaStream
    {
        public int Index { get; internal set; }
        public string CodecName { get; internal set; } = null!;
        public string CodecLongName { get; internal set; } = null!;
        public int BitRate { get; internal set; }
        public TimeSpan Duration { get; internal set; }
    }
}