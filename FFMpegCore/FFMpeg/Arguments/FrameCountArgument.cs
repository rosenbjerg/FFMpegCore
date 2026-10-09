namespace FFMpegCore.Arguments;

/// <summary>
///     Represents frame output count parameter
/// </summary>
public class FrameCountArgument : IArgument
{
    public readonly int Frames;

    public FrameCountArgument(int frames)
    {
        Frames = frames;
    }

    public string Text => $"-frames:v {Frames}";
}
