using System.IO.Compression;
using System.Text.Json;
using FFMpegCore.Extensions.Downloader.Enums;
using FFMpegCore.Extensions.Downloader.Exceptions;
using FFMpegCore.Extensions.Downloader.Extensions;
using FFMpegCore.Extensions.Downloader.Models;

namespace FFMpegCore.Extensions.Downloader.Services;

/// <summary>
///     Service to interact with ffbinaries.com API
/// </summary>
internal class FFbinariesService
{
    /// <summary>
    ///     Get version info from ffbinaries.com
    /// </summary>
    /// <param name="version">use to explicitly state the version of ffmpeg you want</param>
    /// <returns></returns>
    /// <exception cref="FFMpegDownloaderException"></exception>
    internal static async Task<VersionInfo> GetVersionInfo(FFMpegVersions version)
    {
        var versionUri = version.GetDescription();

        HttpClient client = new();
        var response = await client.GetAsync(versionUri);

        if (!response.IsSuccessStatusCode)
        {
            throw new FFMpegDownloaderException($"Failed to get version info from {versionUri}", "network error");
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        var versionInfo = JsonSerializer.Deserialize<VersionInfo>(jsonString);

        return versionInfo ??
               throw new FFMpegDownloaderException($"Failed to deserialize version info from {versionUri}", jsonString);
    }

    /// <summary>
    ///     Download file from uri
    /// </summary>
    /// <param name="address">uri of the file</param>
    /// <returns></returns>
    internal static async Task<Stream> DownloadFileAsSteam(Uri address)
    {
        var client = new HttpClient();
        return await client.GetStreamAsync(address);
    }

    /// <summary>
    ///     Extracts the binaries from the zip stream and saves them to the current binary folder
    /// </summary>
    /// <param name="zipStream">steam of the zip file</param>
    /// <returns></returns>
    internal static IEnumerable<string> ExtractZipAndSave(Stream zipStream)
    {
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        List<string> files = new();
        foreach (var entry in archive.Entries)
        {
            if (entry.Name is "ffmpeg" or "ffmpeg.exe" or "ffprobe.exe" or "ffprobe" or "ffplay.exe" or "ffplay")
            {
                entry.ExtractToFile(Path.Combine(GlobalFFOptions.Current.BinaryFolder, entry.Name), true);
                files.Add(Path.Combine(GlobalFFOptions.Current.BinaryFolder, entry.Name));
            }
        }

        return files;
    }
}
