namespace FFMpegCore.Arguments.Formats.segment;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg-formats.html#segment_002c-stream_005fsegment_002c-ssegment" />
/// Set segment duration to time, the value must be a duration specification. Default value is "2". See also the segment_times option.
/// Note that splitting may not be accurate, unless you force the reference stream key-frames at the given time. See the introductory notice and the examples below.
/// </summary>
public sealed class SegmentTimeArgument(TimeSpan duration) : ISegmentMuxerArgument
{
    private readonly long _duration = Convert.ToInt64(duration.TotalSeconds);

    public string Text => $"-segment_time {_duration}";
}
