using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Pipes
{
    public interface IPipeSink
    {
        Task ReadAsync(Stream inputStream, CancellationToken cancellationToken);
        string GetFormat();
    }
}
