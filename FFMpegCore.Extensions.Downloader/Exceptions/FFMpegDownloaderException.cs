using FFMpegCore.Exceptions;

namespace FFMpegCore.Extensions.Downloader.Exceptions;

public class FFMpegDownloaderException : FFMpegException
{
    public FFMpegDownloaderException(string message) : base(message)
    {
        Detail = string.Empty;
    }

    public FFMpegDownloaderException(string message, string detail) : base(message)
    {
        Detail = detail;
    }

    public string Detail { get; }
}
