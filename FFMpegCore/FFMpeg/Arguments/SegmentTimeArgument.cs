namespace FFMpegCore.Arguments;

/// <summary>
///     Represents segment_time parameter
/// </summary>
public class SegmentTimeArgument : ISegmentArgument
{
    public readonly int Time;

    /// <summary>
    ///     Represents segment_time parameter
    /// </summary>
    /// <param name="time">time in seconds of the segment</param>
    public SegmentTimeArgument(int time)
    {
        Time = time;
    }

    public string Key { get; } = "segment_time";
    public string Value => Time <= 0 ? string.Empty : $"-segment_time {Time}";
}
