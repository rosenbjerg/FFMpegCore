using FFMpegCore.Extensions.Downloader.Enums;
using FFMpegCore.Extensions.Downloader.Exceptions;
using FFMpegCore.Extensions.Downloader.Services;

namespace FFMpegCore.Extensions.Downloader;

public class FFMpegDownloader
{
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
        SupportedPlatforms? platformOverride = null)
    {
        // get all available versions
        var versionInfo = await FFbinariesService.GetVersionInfo(version);

        // get the download info for the current platform
        var downloadInfo = versionInfo.BinaryInfo?.GetCompatibleDownloadInfo(platformOverride) ??
                           throw new FFMpegDownloaderException("Failed to get compatible download info");

        var successList = new List<string>();

        // download ffmpeg if selected
        if (binaries.HasFlag(FFMpegBinaries.FFMpeg) && downloadInfo.FFMpeg is not null)
        {
            var zipStream = FFbinariesService.DownloadFileAsSteam(new Uri(downloadInfo.FFMpeg));
            successList.AddRange(FFbinariesService.ExtractZipAndSave(zipStream));
        }

        // download ffprobe if selected
        if (binaries.HasFlag(FFMpegBinaries.FFProbe) && downloadInfo.FFProbe is not null)
        {
            var zipStream = FFbinariesService.DownloadFileAsSteam(new Uri(downloadInfo.FFProbe));
            successList.AddRange(FFbinariesService.ExtractZipAndSave(zipStream));
        }

        // download ffplay if selected
        if (binaries.HasFlag(FFMpegBinaries.FFPlay) && downloadInfo.FFPlay is not null)
        {
            var zipStream = FFbinariesService.DownloadFileAsSteam(new Uri(downloadInfo.FFPlay));
            successList.AddRange(FFbinariesService.ExtractZipAndSave(zipStream));
        }

        return successList;
    }
}
