using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFMpegCore.Extensions.Downloader;

/// <summary>
///     Downloads the latest FFMpeg suite binaries from ffbinaries.com.
/// </summary>
public class FFMpegDownloader
{
    [Flags]
    public enum FFMpegBinaries : ushort
    {
        FFMpeg,
        FFProbe,
        FFPlay
    }

    public enum FFMpegVersions : ushort
    {
        Latest,
        V6_1,
        V5_1,
        V4_4_1,
        V4_2_1,
        V4_2,
        V4_1,
        V4_0,
        V3_4,
        V3_3,
        V3_2
    }

    public enum PlatformOverride : short
    {
        Windows64,
        Windows32,
        Linux64,
        Linux32,
        LinuxArmhf,
        LinuxArmel,
        LinuxArm64,
        Osx64
    }

    /// <summary>
    ///     Download the latest FFMpeg suite binaries for current platform
    /// </summary>
    /// <param name="version">used to explicitly state the version of binary you want to download</param>
    /// <param name="binaries">used to explicitly state the binaries you want to download (ffmpeg, ffprobe, ffplay)</param>
    /// <param name="platformOverride">used to explicitly state the os and architecture you want to download</param>
    /// <returns>a list of the binaries that have been successfully downloaded</returns>
    public static async Task<List<string>> DownloadFFMpegSuite(
        FFMpegVersions version = FFMpegVersions.Latest,
        FFMpegBinaries binaries = FFMpegBinaries.FFMpeg | FFMpegBinaries.FFProbe,
        PlatformOverride? platformOverride = null)
    {
        var versionInfo = await GetVersionInfo(version);
        var downloadInfo = versionInfo.BinaryInfo?.GetCompatibleDownloadInfo(platformOverride) ??
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
    private static IEnumerable<string> ExtractZipAndSave(Stream zipStream)
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
        [JsonPropertyName("ffmpeg")] public string? FFMpeg { get; set; }

        [JsonPropertyName("ffprobe")] public string? FFProbe { get; set; }

        [JsonPropertyName("ffplay")] public string? FFPlay { get; set; }
    }

    private class BinaryInfo
    {
        [JsonPropertyName("windows-64")] public DownloadInfo? Windows64 { get; set; }

        [JsonPropertyName("windows-32")] public DownloadInfo? Windows32 { get; set; }

        [JsonPropertyName("linux-32")] public DownloadInfo? Linux32 { get; set; }

        [JsonPropertyName("linux-64")] public DownloadInfo? Linux64 { get; set; }

        [JsonPropertyName("linux-armhf")] public DownloadInfo? LinuxArmhf { get; set; }

        [JsonPropertyName("linux-armel")] public DownloadInfo? LinuxArmel { get; set; }

        [JsonPropertyName("linux-arm64")] public DownloadInfo? LinuxArm64 { get; set; }

        [JsonPropertyName("osx-64")] public DownloadInfo? Osx64 { get; set; }

        /// <summary>
        ///     Automatically get the compatible download info for current os and architecture
        /// </summary>
        /// <param name="platformOverride"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public DownloadInfo? GetCompatibleDownloadInfo(PlatformOverride? platformOverride = null)
        {
            if (platformOverride is not null)
            {
                return platformOverride switch
                {
                    PlatformOverride.Windows64 => Windows64,
                    PlatformOverride.Windows32 => Windows32,
                    PlatformOverride.Linux64 => Linux64,
                    PlatformOverride.Linux32 => Linux32,
                    PlatformOverride.LinuxArmhf => LinuxArmhf,
                    PlatformOverride.LinuxArmel => LinuxArmel,
                    PlatformOverride.LinuxArm64 => LinuxArm64,
                    PlatformOverride.Osx64 => Osx64,
                    _ => throw new ArgumentOutOfRangeException(nameof(platformOverride), platformOverride, null)
                };
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return RuntimeInformation.OSArchitecture == Architecture.X64 ? Windows64 : Windows32;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return RuntimeInformation.OSArchitecture switch
                {
                    Architecture.X86 => Linux32,
                    Architecture.X64 => Linux64,
                    Architecture.Arm => LinuxArmhf,
                    Architecture.Arm64 => LinuxArm64,
                    _ => LinuxArmel
                };
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Osx64;
            }

            throw new PlatformNotSupportedException("Unsupported OS or Architecture");
        }
    }

    private class VersionInfo
    {
        [JsonPropertyName("version")] public string? Version { get; set; }

        [JsonPropertyName("permalink")] public string? Permalink { get; set; }

        [JsonPropertyName("bin")] public BinaryInfo? BinaryInfo { get; set; }
    }

    private static readonly Dictionary<FFMpegVersions, string> _FFBinariesAPIs = new()
    {
        { FFMpegVersions.Latest, "https://ffbinaries.com/api/v1/version/latest" },
        { FFMpegVersions.V6_1, "https://ffbinaries.com/api/v1/version/6.1" },
        { FFMpegVersions.V5_1, "https://ffbinaries.com/api/v1/version/5.1" },
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
        if (!_FFBinariesAPIs.TryGetValue(version, out var versionUri))
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
