using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Pipes
{
    /// <summary>
    /// Interface for ffmpeg pipe source data IO
    /// </summary>
    public interface IPipeDataWriter
    {
        string GetFormat();
        Task CopyAsync(System.IO.Stream outputStream, CancellationToken cancellationToken);
    }
}
