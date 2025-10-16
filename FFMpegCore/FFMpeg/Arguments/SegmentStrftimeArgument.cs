namespace FFMpegCore.Arguments;

/// <summary>
///     Use the strftime function to define the name of the new segments to write. If this is selected, the output segment name must contain a
///     strftime function template. Default value is 0.
/// </summary>
public class SegmentStrftimeArgument : ISegmentArgument
{
    public readonly bool Enable;

    /// <summary>
    ///     Use the strftime function to define the name of the new segments to write. If this is selected, the output segment name must contain a
    ///     strftime function template. Default value is 0.
    /// </summary>
    /// <param name="enable">true to enable strftime</param>
    public SegmentStrftimeArgument(bool enable)
    {
        Enable = enable;
    }

    public string Key { get; } = "strftime";
    public string Value => Enable ? "-strftime 1" : string.Empty;
}
