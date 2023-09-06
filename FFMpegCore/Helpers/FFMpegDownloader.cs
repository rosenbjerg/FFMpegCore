using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFMpegCore.Helpers;

/// <summary>
///     Downloads the latest FFMpeg suite binaries from ffbinaries.com.
/// </summary>
public class FFMpegDownloader
{
    [Flags]
    public enum FFMpegBinaries : ushort
    {
        /// <summary>
        ///     FFMpeg binary
        /// </summary>
        FFMpeg,

        /// <summary>
        ///     FFProbe binary
        /// </summary>
        FFProbe,

        /// <summary>
        ///     FFPlay binary
        /// </summary>
        FFPlay
    }

    public enum FFMpegVersions : ushort
    {
        Latest,
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
    ///     Download the latest FFMpeg suite binaries for current platform
    /// </summary>
    /// <param name="version">used to explicitly state the version of binary you want to download</param>
    /// <param name="binaries">used to explicitly state the binary you want to download</param>
    /// <returns>a list of the binaries that have been successfully downloaded</returns>
    public static async Task<List<string>> DownloadFFMpegSuite(FFMpegVersions version = FFMpegVersions.Latest,
        FFMpegBinaries binaries = FFMpegBinaries.FFMpeg | FFMpegBinaries.FFProbe)
    {
        var versionInfo = await GetVersionInfo(version);
        var downloadInfo = versionInfo.BinaryInfo?.GetCompatibleDownloadInfo() ??
                           throw new FFMpegDownloaderException("Failed to get compatible download info");

        var successList = new List<string>();

        // if ffmpeg is selected
        if (binaries.HasFlag(FFMpegBinaries.FFMpeg) && downloadInfo.FFMpeg is not null)
        {
            var zipStream = DownloadFileAsSteam(new Uri(downloadInfo.FFMpeg));
            successList.AddRange(ExtractZipAndSave(zipStream));
        }

        // if ffprobe is selected
        if (binaries.HasFlag(FFMpegBinaries.FFProbe) && downloadInfo.FFProbe is not null)
        {
            var zipStream = DownloadFileAsSteam(new Uri(downloadInfo.FFProbe));
            successList.AddRange(ExtractZipAndSave(zipStream));
        }

        // if ffplay is selected
        if (binaries.HasFlag(FFMpegBinaries.FFPlay) && downloadInfo.FFPlay is not null)
        {
            var zipStream = DownloadFileAsSteam(new Uri(downloadInfo.FFPlay));
            successList.AddRange(ExtractZipAndSave(zipStream));
        }

        return successList;
    }

    /// <summary>
    ///     Download file from uri
    /// </summary>
    /// <param name="address">uri of the file</param>
    /// <returns></returns>
    private static MemoryStream DownloadFileAsSteam(Uri address)
    {
        var client = new WebClient();
        var fileStream = new MemoryStream(client.DownloadData(address));
        fileStream.Position = 0;

        return fileStream;
    }

    /// <summary>
    ///     Extracts the binaries from the zip stream and saves them to the current binary folder
    /// </summary>
    /// <param name="zipStream">steam of the zip file</param>
    /// <returns></returns>
    private static List<string> ExtractZipAndSave(Stream zipStream)
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

    #region FFbinaries api

    private class DownloadInfo
    {
        [JsonPropertyName("ffmpeg")] public string? FFMpeg { get; }

        [JsonPropertyName("ffprobe")] public string? FFProbe { get; }

        [JsonPropertyName("ffplay")] public string? FFPlay { get; }
    }

    private class BinaryInfo
    {
        [JsonPropertyName("windows-64")] public DownloadInfo? Windows64 { get; }

        [JsonPropertyName("windows-32")] public DownloadInfo? Windows32 { get; }

        [JsonPropertyName("linux-32")] public DownloadInfo? Linux32 { get; set; }

        [JsonPropertyName("linux-64")] public DownloadInfo? Linux64 { get; }

        [JsonPropertyName("linux-armhf")] public DownloadInfo? LinuxArmhf { get; }

        [JsonPropertyName("linux-armel")] public DownloadInfo? LinuxArmel { get; set; }

        [JsonPropertyName("linux-arm64")] public DownloadInfo? LinuxArm64 { get; }

        [JsonPropertyName("osx-64")] public DownloadInfo? Osx64 { get; }

        public DownloadInfo? GetCompatibleDownloadInfo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return RuntimeInformation.OSArchitecture == Architecture.X64 ? Windows64 : Windows32;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return RuntimeInformation.OSArchitecture switch
                {
                    Architecture.X64 => Linux64,
                    Architecture.Arm => LinuxArmhf,
                    Architecture.Arm64 => LinuxArm64,
                    _ => throw new PlatformNotSupportedException("Unsupported Linux architecture")
                };
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Osx64;
            }

            throw new PlatformNotSupportedException("Unsupported OS");
        }
    }

    private class VersionInfo
    {
        [JsonPropertyName("version")] public string? Version { get; set; }

        [JsonPropertyName("permalink")] public string? Permalink { get; set; }

        [JsonPropertyName("bin")] public BinaryInfo? BinaryInfo { get; set; }
    }

    private static readonly Dictionary<FFMpegVersions, string> FFBinariesAPIs = new()
    {
        { FFMpegVersions.Latest, "https://ffbinaries.com/api/v1/version/latest" },
        { FFMpegVersions.V4_4_1, "https://ffbinaries.com/api/v1/version/4.4.1" },
        { FFMpegVersions.V4_2_1, "https://ffbinaries.com/api/v1/version/4.2.1" },
        { FFMpegVersions.V4_2, "https://ffbinaries.com/api/v1/version/4.2" },
        { FFMpegVersions.V4_1, "https://ffbinaries.com/api/v1/version/4.1" },
        { FFMpegVersions.V4_0, "https://ffbinaries.com/api/v1/version/4.0" },
        { FFMpegVersions.V3_4, "https://ffbinaries.com/api/v1/version/3.4" },
        { FFMpegVersions.V3_3, "https://ffbinaries.com/api/v1/version/3.3" },
        { FFMpegVersions.V3_2, "https://ffbinaries.com/api/v1/version/3.2" }
    };

    /// <summary>
    ///     Get version info from ffbinaries.com
    /// </summary>
    /// <param name="version">use to explicitly state the version of ffmpeg you want</param>
    /// <returns></returns>
    /// <exception cref="FFMpegDownloaderException"></exception>
    private static async Task<VersionInfo> GetVersionInfo(FFMpegVersions version)
    {
        if (!FFBinariesAPIs.TryGetValue(version, out var versionUri))
        {
            throw new FFMpegDownloaderException($"Invalid version selected: {version}", "contact dev");
        }

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

    #endregion
}

/// <summary>
///     Custom exception for FFMpegDownloader
/// </summary>
public class FFMpegDownloaderException : Exception
{
    public FFMpegDownloaderException(string message) : base(message)
    {
    }

    public FFMpegDownloaderException(string message, string detail) : base(message)
    {
        Detail = detail;
    }

    public string Detail { get; set; } = "";
}
