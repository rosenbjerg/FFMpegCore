namespace FFMpegCore.Arguments;

public class NullOutputArgument : IOutputArgument
{
    public string Text => "-f null -";

    public void Pre(FFOptions options) { }

    public Task During(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Post() { }
}
