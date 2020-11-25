using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Pipes
{
    public interface IPipeSink
    {
        Task ReadAsync(System.IO.Stream inputStream, CancellationToken cancellationToken);
        string GetFormat();
    }
}
