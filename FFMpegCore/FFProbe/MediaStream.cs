using System;

namespace FFMpegCore.FFMPEG
{
    public class MediaStream
    {
        public int Index { get; internal set; }
        public string CodecName { get; internal set; }
        public string CodecLongName { get; internal set; }
        public int BitRate { get; internal set; }
        public TimeSpan Duration { get; internal set; }
    }
}