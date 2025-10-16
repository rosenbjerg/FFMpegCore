namespace FFMpegCore.Arguments;

/// <summary>
///     Represents reset_timestamps parameter
/// </summary>
public class SegmentResetTimeStampsArgument : ISegmentArgument
{
    public readonly bool ResetTimestamps;

    /// <summary>
    ///     Represents reset_timestamps parameter
    /// </summary>
    /// <param name="resetTimestamps">true if files timestamps are to be reset</param>
    public SegmentResetTimeStampsArgument(bool resetTimestamps)
    {
        ResetTimestamps = resetTimestamps;
    }

    public string Key { get; } = "reset_timestamps";
    public string Value => ResetTimestamps ? "-reset_timestamps 1" : string.Empty;
}
