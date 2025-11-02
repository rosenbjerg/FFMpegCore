namespace FFMpegCore.Arguments.Formats;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg-formats.html#Format-Options"/>
/// Set probing size in bytes, i.e. the size of the data to analyze to get stream information.
/// A higher value will enable detecting more information in case it is dispersed into the stream, but will increase latency.
/// Must be an integer not lesser than 32. It is 5000000 by default.
/// </summary>
public sealed class ProbeSizeArgument(long probesize) : IArgument
{
    public string Text => $"-probesize {probesize}";
}
