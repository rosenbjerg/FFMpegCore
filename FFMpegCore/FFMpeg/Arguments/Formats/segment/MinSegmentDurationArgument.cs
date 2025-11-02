namespace FFMpegCore.Arguments.Formats.segment;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg-formats.html#segment_002c-stream_005fsegment_002c-ssegment" />
/// Set minimum segment duration to time, the value must be a duration specification.
/// This prevents the muxer ending segments at a duration below this value.
/// Only effective with segment_time. Default value is "0".
/// </summary>
public sealed class MinSegmentDurationArgument(TimeSpan duration) : ISegmentMuxerArgument
{
    private readonly long _duration = Convert.ToInt64(duration.TotalSeconds);

    public string Text => $"-min_seg_duration {_duration}";
}
