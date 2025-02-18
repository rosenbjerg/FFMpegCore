namespace FFMpegCore.Extensions.Downloader.Exceptions;

/// <summary>
///     Custom exception for FFMpegDownloader
/// </summary>
public class FFMpegDownloaderException : Exception
{
    public string Detail { get; set; } = "";

    public FFMpegDownloaderException(string message) : base(message)
    {
    }

    public FFMpegDownloaderException(string message, string detail) : base(message)
    {
        Detail = detail;
    }
}
