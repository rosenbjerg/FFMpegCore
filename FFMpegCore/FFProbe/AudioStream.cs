namespace FFMpegCore;

public class AudioStream : MediaStream
{
    internal AudioStream() { }

    public int Channels { get; internal set; }
    public string ChannelLayout { get; internal set; } = null!;
    public int SampleRateHz { get; internal set; }
    public string Profile { get; internal set; } = null!;
}
