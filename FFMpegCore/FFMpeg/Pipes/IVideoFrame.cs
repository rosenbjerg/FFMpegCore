using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Pipes
{
    /// <summary>
    /// Interface for Video frame
    /// </summary>
    public interface IVideoFrame
    {
        int Width { get; }
        int Height { get; }
        string Format { get; }

        void Serialize(Stream pipe);
        Task SerializeAsync(Stream pipe, CancellationToken token);
    }
}
