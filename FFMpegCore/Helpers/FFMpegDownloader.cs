using System.Net;
using System.IO.Compression;


namespace FFMpegCore.Helpers;
using System.Runtime.InteropServices;

/// <summary>
/// Downloads the latest FFMpeg suite binaries from GitHub. Only supported for windows at the moment.
/// </summary>
public class FFMpegDownloader
{
    private static Dictionary<FFMpegVersions, string> Windows64FFMpegDownloadUrls = new()
    {
        { FFMpegVersions.V4_4_1, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.4.1/ffmpeg-4.4.1-win-64.zip"},
        { FFMpegVersions.V4_2_1, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.2.1/ffmpeg-4.2.1-win-64.zip"},
        { FFMpegVersions.V4_2, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.2/ffmpeg-4.2-win-64.zip"},
        { FFMpegVersions.V4_1, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.1/ffmpeg-4.1-win-64.zip"},
        { FFMpegVersions.V4_0, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.0/ffmpeg-4.0.1-win-64.zip"},
        { FFMpegVersions.V3_4, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v3.4/ffmpeg-3.4-win-64.zip"},
        { FFMpegVersions.V3_3, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v3.3/ffmpeg-3.3.4-win-64.zip"},
        { FFMpegVersions.V3_2, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v3.2/ffmpeg-3.2-win-64.zip"},
    };
    
    private static Dictionary<FFMpegVersions, string> Windows32FFMpegDownloadUrls = new()
    {
        { FFMpegVersions.V4_4_1, "" },
        { FFMpegVersions.V4_2_1, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.2.1/ffmpeg-4.2.1-win-32.zip"},
        { FFMpegVersions.V4_2, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.2/ffmpeg-4.2-win-32.zip"},
        { FFMpegVersions.V4_1, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.1/ffmpeg-4.1-win-32.zip"},
        { FFMpegVersions.V4_0, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.0/ffmpeg-4.0.1-win-32.zip"},
        { FFMpegVersions.V3_4, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v3.4/ffmpeg-3.4-win-32.zip"},
        { FFMpegVersions.V3_3, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v3.3/ffmpeg-3.3.4-win-32.zip"},
        { FFMpegVersions.V3_2, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v3.2/ffmpeg-3.2-win-32.zip"},
    };
    
    /// <summary>
    /// Supported FFMpeg versions
    /// </summary>
    public enum FFMpegVersions
    {
        V4_4_1,
        V4_2_1,
        V4_2,
        V4_1,
        V4_0,
        V3_4,
        V3_3,
        V3_2
    }

    /// <summary>
    /// Downloads the latest FFMpeg suite binaries to bin directory.
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static List<string> AutoDownloadFFMpegSuite(FFMpegVersions version = FFMpegVersions.V4_4_1)
    {
        var files = AutoDownloadFFMpeg(version);
        files.AddRange(AutoDownloadFFProbe(version));
        files.AddRange(AutoDownloadFFPlay(version));
        
        return files;
    }

    /// <summary>
    /// Downloads the latest FFMpeg binaries to bin directory.
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static List<string> AutoDownloadFFMpeg(FFMpegVersions version = FFMpegVersions.V4_4_1)
    {
        var url = Environment.Is64BitProcess
            ? new Uri(Windows64FFMpegDownloadUrls[version])
            : new Uri(Windows32FFMpegDownloadUrls[version]);
        
        HasValidUri(url);

        Stream zipStream = DownloadZip(url);

        return ExtractAndSave(zipStream);
    }
    
    /// <summary>
    /// Downloads the latest FFProbe binaries to bin directory.
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static List<string> AutoDownloadFFProbe(FFMpegVersions version = FFMpegVersions.V4_4_1)
    {
        var url = Environment.Is64BitProcess
            ? new Uri(Windows64FFMpegDownloadUrls[version].Replace("ffmpeg", "ffprobe"))
            : new Uri(Windows32FFMpegDownloadUrls[version].Replace("ffmpeg", "ffprobe"));
        
        HasValidUri(url);
        
        Stream zipStream = DownloadZip(url);

        return ExtractAndSave(zipStream);
    }
    
    /// <summary>
    /// Downloads the latest FFPlay binaries to bin directory.
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static List<string> AutoDownloadFFPlay(FFMpegVersions version = FFMpegVersions.V4_4_1)
    {
        var url = Environment.Is64BitProcess
            ? new Uri(Windows64FFMpegDownloadUrls[version].Replace("ffmpeg", "ffplay"))
            : new Uri(Windows32FFMpegDownloadUrls[version].Replace("ffmpeg", "ffplay"));
        
        HasValidUri(url);

        Stream zipStream = DownloadZip(url);

        return ExtractAndSave(zipStream);
    }

    /// <summary>
    /// Downloads the zip file from the given url.
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    private static MemoryStream DownloadZip(Uri address)
    {
        var client = new WebClient();
        var zipStream = new MemoryStream(client.DownloadData(address));
        zipStream.Position = 0;

        return zipStream;
    }
    
    /// <summary>
    /// Extracts the zip file and saves the binaries to bin directory.
    /// </summary>
    /// <param name="zipStream"></param>
    /// <returns></returns>
    private static List<string> ExtractAndSave(Stream zipStream)
    {
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        List<string> files = new();
        foreach (var entry in archive.Entries)
        {
            if (entry.Name is "ffmpeg.exe" or "ffmpeg" or "ffprobe.exe" or "ffplay.exe" or "ffplay") // only extract the binaries
            {
                entry.ExtractToFile(Path.Combine(GlobalFFOptions.Current.BinaryFolder, entry.Name), true);
                files.Add(Path.Combine(GlobalFFOptions.Current.BinaryFolder, entry.Name));
            }
        }

        return files;
    }

    /// <summary>
    /// Checks if the given uri is valid.
    /// </summary>
    /// <param name="uri"></param>
    /// <exception cref="PlatformNotSupportedException"></exception>
    private static void HasValidUri(Uri uri)
    {
        if (uri.ToString() == "" || !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("The requested version of FFMpeg component is not available for your OS/System.");
        }
    }
}
