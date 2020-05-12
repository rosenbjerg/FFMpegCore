using System;

namespace FFMpegCore
{
    public abstract class SimpleStream
    {
        public int Index { get; internal set; }
        public string CodecName { get; internal set; } = null!;
        public string CodecLongName { get; internal set; } = null!;
        public TimeSpan Duration { get; internal set; }
        public string? Language { get; internal set; }
    }
}