namespace FFMpegCore.Arguments;

public class StreamLoopArgument : IArgument
{
    public readonly int Count;

    public StreamLoopArgument(int count)
    {
        Count = count;
    }

    public string Text => $"-stream_loop {Count}";
}
