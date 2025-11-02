namespace FFMpegCore.Arguments.MainOptions;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg.html#Main-options" />
/// Explicitly disable logging of encoding progress/statistics.
/// </summary>
public sealed class Stats(bool value) : BaseOptionArgument(value)
{
    protected override string ArgumentName => "stats";
}
