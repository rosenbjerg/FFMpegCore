namespace FFMpegCore.Arguments;

public interface IInputOutputArgument : IArgument
{
    void Pre(FFOptions options);
    Task During(CancellationToken cancellationToken = default);
    void Post();
}
