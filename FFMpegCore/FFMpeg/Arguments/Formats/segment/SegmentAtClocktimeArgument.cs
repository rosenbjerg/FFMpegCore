namespace FFMpegCore.Arguments.Formats.segment;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg-formats.html#segment_002c-stream_005fsegment_002c-ssegment" />
/// If set to "1" split at regular clock time intervals starting from 00:00 o’clock.
/// The time value specified in segment_time is used for setting the length of the splitting interval.
/// For example with segment_time set to "900" this makes it possible to create files at 12:00 o’clock, 12:15, 12:30, etc.
/// Default value is "0".
/// </summary>
public sealed class SegmentAtClocktimeArgument(bool value) : BaseBoolArgument(value), ISegmentMuxerArgument
{
    protected override string ArgumentName => "segment_atclocktime";
}
