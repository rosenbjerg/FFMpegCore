namespace FFMpegCore.Pipes
{
    public interface IPipeSink
    {
        Task ReadAsync(Stream inputStream, CancellationToken cancellationToken);
        string GetFormat();
    }
}
