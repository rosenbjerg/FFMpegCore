using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Pipes
{
    public interface IPipeDataReader
    {
        Task CopyAsync(System.IO.Stream inputStream, CancellationToken cancellationToken);
        string GetFormat();
    }
}
