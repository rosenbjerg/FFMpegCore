using System.ComponentModel;
using System.Net;
using System.IO;
using System.IO.Compression;


namespace FFMpegCore.Helpers;
using System.Runtime.InteropServices;

/// <summary>
/// Downloads the latest FFMpeg suite binaries from GitHub. Only supported for windows at the moment.
/// </summary>
public class FFMpegDownloader // this class is built to be easily modified to support other platforms
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
        { FFMpegVersions.V4_4_1, "https://example.com/" },
        { FFMpegVersions.V4_2_1, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.2.1/ffmpeg-4.2.1-win-32.zip"},
        { FFMpegVersions.V4_2, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.2/ffmpeg-4.2-win-32.zip"},
        { FFMpegVersions.V4_1, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.1/ffmpeg-4.1-win-32.zip"},
        { FFMpegVersions.V4_0, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.0/ffmpeg-4.0.1-win-32.zip"},
        { FFMpegVersions.V3_4, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v3.4/ffmpeg-3.4-win-32.zip"},
        { FFMpegVersions.V3_3, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v3.3/ffmpeg-3.3.4-win-32.zip"},
        { FFMpegVersions.V3_2, "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v3.2/ffmpeg-3.2-win-32.zip"},
    };
    
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

    public static List<string> AutoDownloadFFMpegSuite(FFMpegVersions version = FFMpegVersions.V4_4_1)
    {
        var files = AutoDownloadFFMpeg(version);
        files.AddRange(AutoDownloadFFProbe(version));
        files.AddRange(AutoDownloadFFPlay(version));
        
        return files;
    }

    public static List<string> AutoDownloadFFMpeg(FFMpegVersions version = FFMpegVersions.V4_4_1)
    {
        var url = Environment.Is64BitProcess
            ? new Uri(Windows64FFMpegDownloadUrls[version])
            : new Uri(Windows32FFMpegDownloadUrls[version]);
        
        HasValidUri(url);

        Stream zipStream = DownloadZip(url);

        return ExtractAndSave(zipStream);
    }
    
    public static List<string> AutoDownloadFFProbe(FFMpegVersions version = FFMpegVersions.V4_4_1)
    {
        var url = Environment.Is64BitProcess
            ? new Uri(Windows64FFMpegDownloadUrls[version].Replace("ffmpeg", "ffprobe"))
            : new Uri(Windows32FFMpegDownloadUrls[version].Replace("ffmpeg", "ffprobe"));
        
        HasValidUri(url);
        
        Stream zipStream = DownloadZip(url);

        return ExtractAndSave(zipStream);
    }
    
    public static List<string> AutoDownloadFFPlay(FFMpegVersions version = FFMpegVersions.V4_4_1)
    {
        var url = Environment.Is64BitProcess
            ? new Uri(Windows64FFMpegDownloadUrls[version].Replace("ffmpeg", "ffplay"))
            : new Uri(Windows32FFMpegDownloadUrls[version].Replace("ffmpeg", "ffplay"));
        
        HasValidUri(url);

        Stream zipStream = DownloadZip(url);

        return ExtractAndSave(zipStream);
    }

    private static MemoryStream DownloadZip(Uri address)
    {
        var client = new WebClient();
        var zipStream = new MemoryStream(client.DownloadData(address));
        zipStream.Position = 0;

        return zipStream;
    }
    
    private static List<string> ExtractAndSave(Stream zipStream)
    {
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        List<string> files = new();
        foreach (var entry in archive.Entries)
        {
            if (entry.Name is "ffmpeg.exe" or "ffmpeg" or "ffprobe.exe")
            {
                entry.ExtractToFile(Path.Combine(GlobalFFOptions.Current.BinaryFolder, entry.Name), true);
                files.Add(Path.Combine(GlobalFFOptions.Current.BinaryFolder, entry.Name));
            }
        }

        return files;
    }

    private static void HasValidUri(Uri uri)
    {
        if (uri.ToString() == "https://example.com/" || !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("The requested version of FFMpeg component is not available for your OS.");
        }
    }
}
