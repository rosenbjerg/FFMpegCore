namespace FFMpegCore.Arguments;

public class OutputNullArgument : IOutputArgument
{
    public string Text => "-f null -";

    public Task During(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Post()
    {
    }

    public void Pre()
    {
    }
}
