namespace FFMpegCore;

public class MediaFormat : ITagsContainer
{
    internal MediaFormat() { }

    public TimeSpan Duration { get; internal set; }
    public TimeSpan StartTime { get; internal set; }
    public string FormatName { get; internal set; } = null!;
    public string FormatLongName { get; internal set; } = null!;
    public int StreamCount { get; internal set; }
    public double ProbeScore { get; internal set; }
    public long BitRate { get; internal set; }
    public IReadOnlyDictionary<string, string>? Tags { get; internal set; }
}
