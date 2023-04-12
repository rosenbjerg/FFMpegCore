using System.ComponentModel;
using System.Net;
using System.IO;
using System.IO.Compression;


namespace FFMpegCore.Helpers;
using System.Runtime.InteropServices;

/// <summary>
/// Downloads the latest FFMpeg binaries from GitHub. Only supported for windows at the moment.
/// </summary>
public class FFMpegDownloader // this class is built to be easily modified to support other platforms
{
    /// <summary>
    /// List of URLs to download FFMpeg from.
    /// </summary>
    private static Dictionary<string, string> FFMpegDownloadUrls = new()
    {
        { "windows", "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip" }
    };

    public static List<string> GetLatestVersion()
    {
        var os = GetOSPlatform();
        var zipStream = DownloadFFMpeg(new Uri(FFMpegDownloadUrls[os]));
        return ExtractAndSave(zipStream);
    }

    private static MemoryStream DownloadFFMpeg(Uri address)
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
    
    private static string GetOSPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "windows";
        }

        throw new PlatformNotSupportedException("Auto download is only supported on Windows.");
    }
}
