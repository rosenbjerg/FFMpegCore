namespace FFMpegCore.Arguments;

public class GopSizeArgument : IArgument
{
    public readonly int Frames;

    public GopSizeArgument(int frames)
    {
        Frames = frames;
    }

    public string Text => $"-g {Frames}";
}
