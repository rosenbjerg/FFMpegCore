namespace FFMpegCore.Arguments.VideoFilters;

/// <summary>
/// <see href="https://www.ffmpeg.org/ffmpeg-filters.html#format-1" />
/// Convert the input video to one of the specified pixel formats.
/// Libavfilter will try to pick one that is suitable as input to the next filter.
/// </summary>
public sealed class VideoFilterFormatArgument : IVideoFilterArgument
{
    public string Key => "format";
    public string Value { get; }

    private VideoFilterFormatArgument(string pixelFormat)
    {
        Value = $"pix_fmts={pixelFormat}";
    }

    public VideoFilterFormatArgument(params ReadOnlySpan<string?> pixelFormats)
#if NETSTANDARD2_1_OR_GREATER || NET8_OR_GREATER
        : this(string.Join('|', pixelFormats))
#else
        : this(string.Join("|", pixelFormats.ToArray()))
#endif
    {
    }
}
