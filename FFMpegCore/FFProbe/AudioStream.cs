namespace FFMpegCore.FFMPEG
{
    public class AudioStream : MediaStream
    {
        public int Channels { get; internal set; }
        public string ChannelLayout { get; internal set; } = null!;
        public int SampleRateHz { get; internal set; }
    }
}