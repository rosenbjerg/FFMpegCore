namespace FFMpegCore.Arguments.Formats.segment;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg-formats.html#segment_002c-stream_005fsegment_002c-ssegment" />
/// </summary>
public sealed class SegmentArgumentOptions
{
    private readonly FFMpegArgumentOptions _options;

    internal SegmentArgumentOptions(FFMpegArgumentOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// <inheritdoc cref="MinSegmentDurationArgument"/>
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    public SegmentArgumentOptions WithMinimumSegmentDuration(TimeSpan duration)
    {
        return WithArgument(new MinSegmentDurationArgument(duration));
    }

    /// <summary>
    /// <inheritdoc cref="SegmentTimeArgument"/>
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    public SegmentArgumentOptions WithSegmentTime(TimeSpan duration)
    {
        return WithArgument(new SegmentTimeArgument(duration));
    }

    /// <summary>
    /// <inheritdoc cref="SegmentAtClocktimeArgument"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public SegmentArgumentOptions WithSegmentAtClocktime(bool value = true)
    {
        return WithArgument(new SegmentAtClocktimeArgument(value));
    }

    /// <summary>
    /// <inheritdoc cref="StrftimeArgument"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public SegmentArgumentOptions WithStrftime(bool value = true)
    {
        return WithArgument(new StrftimeArgument(value));
    }

    /// <summary>
    /// <inheritdoc cref="ResetTimestampsArgument"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public SegmentArgumentOptions WithResetTimestamps(bool value = true)
    {
        return WithArgument(new ResetTimestampsArgument(value));
    }

    public SegmentArgumentOptions WithArgument(ISegmentArgument argument)
    {
        _options.WithArgument(argument);
        return this;
    }
}
