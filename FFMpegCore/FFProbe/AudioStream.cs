namespace FFMpegCore
{
    public class AudioStream : MediaStream
    {
        public int Channels { get; set; }
        public string ChannelLayout { get; set; } = null!;
        public int SampleRateHz { get; set; }
        public string Profile { get; set; } = null!;
    }
}
