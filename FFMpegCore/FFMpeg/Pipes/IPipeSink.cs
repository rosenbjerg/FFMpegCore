using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Pipes
{
    public interface IPipeSink
    {
        Task CopyAsync(System.IO.Stream inputStream, CancellationToken cancellationToken);
        string GetFormat();
    }
}
