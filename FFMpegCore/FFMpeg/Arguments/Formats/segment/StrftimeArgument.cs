namespace FFMpegCore.Arguments.Formats.segment;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg-formats.html#segment_002c-stream_005fsegment_002c-ssegment" />
/// Use the strftime function to define the name of the new segments to write.
/// If this is selected, the output segment name must contain a strftime function template.
/// Default value is 0.
/// </summary>
public sealed class StrftimeArgument(bool value) : BaseBoolArgument(value), ISegmentMuxerArgument
{
    protected override string ArgumentName => "strftime";
}
