namespace FFMpegCore.Arguments.Formats;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg-formats.html#Format-Options"/>
/// Specify how many microseconds are analyzed to probe the input.
/// A higher value will enable detecting more accurate information, but will increase latency.
/// It defaults to 5,000,000 microseconds = 5 seconds. 
/// </summary>
public sealed class AnalyzeDurationArgument(TimeSpan duration) : IArgument
{
#if NET8_OR_GREATER
    private readonly long _duration = Convert.ToInt64(duration.TotalMicroseconds);
#else
    // https://github.com/dotnet/runtime/blob/e8812e7419db9137f20b990786a53ed71e27e11e/src/libraries/System.Private.CoreLib/src/System/TimeSpan.cs#L371
    private readonly long _duration = Convert.ToInt64((double)duration.Ticks / 10);
#endif

    public string Text => $"-analyzeduration {_duration}";
}
