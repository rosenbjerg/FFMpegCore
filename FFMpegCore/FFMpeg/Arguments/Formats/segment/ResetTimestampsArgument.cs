namespace FFMpegCore.Arguments.Formats.segment;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg-formats.html#segment_002c-stream_005fsegment_002c-ssegment" />
/// Reset timestamps at the beginning of each segment, so that each segment will start with near-zero timestamps.
/// It is meant to ease the playback of the generated segments.
/// May not work with some combinations of muxers/codecs.
/// It is set to <c>0</c> by default.
/// </summary>
public sealed class ResetTimestampsArgument(bool value) : BaseBoolArgument(value), ISegmentMuxerArgument
{
    protected override string ArgumentName => "reset_timestamps";
}
