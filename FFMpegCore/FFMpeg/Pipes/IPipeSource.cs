using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Pipes
{
    /// <summary>
    /// Interface for ffmpeg pipe source data IO
    /// </summary>
    public interface IPipeSource
    {
        string GetStreamArguments();
        Task WriteAsync(System.IO.Stream outputStream, CancellationToken cancellationToken);
    }
}
