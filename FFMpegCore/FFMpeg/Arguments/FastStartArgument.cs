namespace FFMpegCore.Arguments;

/// <summary>
///     Faststart argument - for moving moov atom to the start of file
/// </summary>
public class FastStartArgument : IArgument
{
    public string Text => "-movflags faststart";
}
