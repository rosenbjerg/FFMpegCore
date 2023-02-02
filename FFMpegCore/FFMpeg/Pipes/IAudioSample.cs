namespace FFMpegCore.Pipes
{
    /// <summary>
    /// Interface for Audio sample
    /// </summary>
    public interface IAudioSample
    {
        void Serialize(Stream stream);

        Task SerializeAsync(Stream stream, CancellationToken token);
    }
}
