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
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>a list of the binaries that have been successfully downloaded</returns>
    public static async Task<List<string>> DownloadBinariesAsync(
        FFMpegVersions version = FFMpegVersions.LatestAvailable,
        FFMpegBinaries binaries = FFMpegBinaries.FFMpeg | FFMpegBinaries.FFProbe,
        FFOptions? options = null,
        SupportedPlatforms? platformOverride = null,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();

        var versionInfo = await httpClient.GetVersionInfo(version, cancellationToken).ConfigureAwait(false);
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
                using var response = await httpClient
                    .GetAsync(new Uri(binaryUrl), HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                using var zipStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                var extracted = ExtractZipAndSave(zipStream, relevantOptions.BinaryFolder, cancellationToken);
                successList.AddRange(extracted);
            }
        }

        return successList;
    }

    private static async Task<VersionInfo> GetVersionInfo(this HttpClient client, FFMpegVersions version, CancellationToken cancellationToken)
    {
        var versionUri = version.GetDescription();

        var response = await client.GetAsync(versionUri, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new FFMpegDownloaderException($"Failed to get version info from {versionUri}", "network error");
        }

        var jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var versionInfo = JsonSerializer.Deserialize<VersionInfo>(jsonString);

        return versionInfo ??
               throw new FFMpegDownloaderException($"Failed to deserialize version info from {versionUri}", jsonString);
    }

    private static IEnumerable<string> ExtractZipAndSave(Stream zipStream, string binaryFolder, CancellationToken cancellationToken)
    {
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        foreach (var entry in archive.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (entry.Name is "ffmpeg" or "ffmpeg.exe" or "ffprobe.exe" or "ffprobe" or "ffplay.exe" or "ffplay")
            {
                var filePath = Path.Combine(binaryFolder, entry.Name);
                entry.ExtractToFile(filePath, true);
                yield return filePath;
            }
        }
    }
}
