using System.IO;
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
        Task WriteAsync(Stream outputStream, CancellationToken cancellationToken);
    }
}
