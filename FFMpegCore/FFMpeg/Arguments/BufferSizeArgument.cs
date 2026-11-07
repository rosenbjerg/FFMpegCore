namespace FFMpegCore.Arguments;

public class BufferSizeArgument : IArgument
{
    public readonly int Kilobits;

    public BufferSizeArgument(int kilobits)
    {
        Kilobits = kilobits;
    }

    public string Text => $"-bufsize {Kilobits}k";
}
