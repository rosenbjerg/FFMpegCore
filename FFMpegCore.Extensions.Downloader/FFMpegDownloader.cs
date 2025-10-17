using System.IO.Compression;
using System.Text.Json;
using FFMpegCore.Extensions.Downloader.Enums;
using FFMpegCore.Extensions.Downloader.Exceptions;
using FFMpegCore.Extensions.Downloader.Extensions;
using FFMpegCore.Extensions.Downloader.Models;

namespace FFMpegCore.Extensions.Downloader;

public static class FFMpegDownloader
{
    /// <summary>
    ///     Download the latest FFMpeg suite binaries for current platform
    /// </summary>
    /// <param name="version">used to explicitly state the version of binary you want to download</param>
    /// <param name="binaries">used to explicitly state the binaries you want to download (ffmpeg, ffprobe, ffplay)</param>
    /// <param name="options">used for specifying binary folder to download binaries into. If not provided, GlobalFFOptions are used</param>
    /// <param name="platformOverride">used to explicitly state the os and architecture you want to download</param>
    /// <returns>a list of the binaries that have been successfully downloaded</returns>
    public static async Task<List<string>> DownloadFFMpegSuite(
        FFMpegVersions version = FFMpegVersions.Latest,
        FFMpegBinaries binaries = FFMpegBinaries.FFMpeg | FFMpegBinaries.FFProbe,
        FFOptions? options = null,
        SupportedPlatforms? platformOverride = null)
    {
        using var httpClient = new HttpClient();

        var versionInfo = await httpClient.GetVersionInfo(version);
        var binariesDictionary = versionInfo.BinaryInfo?.GetCompatibleDownloadInfo(platformOverride) ??
                                 throw new FFMpegDownloaderException("Failed to get compatible download info");

        var successList = new List<string>();
        var relevantOptions = options ?? GlobalFFOptions.Current;
        if (string.IsNullOrEmpty(relevantOptions.BinaryFolder))
        {
            throw new FFMpegDownloaderException("Binary folder not specified");
        }

        var binaryFlags = binaries.GetFlags();
        foreach (var binaryFlag in binaryFlags)
        {
            if (binariesDictionary.TryGetValue(binaryFlag.ToString().ToLowerInvariant(), out var binaryUrl))
            {
                await using var zipStream = await httpClient.GetStreamAsync(new Uri(binaryUrl));
                var extracted = ExtractZipAndSave(zipStream, relevantOptions.BinaryFolder);
                successList.AddRange(extracted);
            }
        }

        return successList;
    }

    private static async Task<VersionInfo> GetVersionInfo(this HttpClient client, FFMpegVersions version)
    {
        var versionUri = version.GetDescription();

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

    private static IEnumerable<string> ExtractZipAndSave(Stream zipStream, string binaryFolder)
    {
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        List<string> files = new();
        foreach (var entry in archive.Entries)
        {
            if (entry.Name is "ffmpeg" or "ffmpeg.exe" or "ffprobe.exe" or "ffprobe" or "ffplay.exe" or "ffplay")
            {
                var filePath = Path.Combine(binaryFolder, entry.Name);
                entry.ExtractToFile(filePath, true);
                files.Add(filePath);
            }
        }

        return files;
    }
}
