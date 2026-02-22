using FFMpegCore.Exceptions;
using Instances;

namespace FFMpegCore.Helpers;

public static class FFMpegHelper
{
    private static bool _ffmpegVerified;
    private static readonly object _syncObject = new();

    public static void ConversionSizeExceptionCheck(IMediaAnalysis info)
    {
        ConversionSizeExceptionCheck(info.PrimaryVideoStream!.Width, info.PrimaryVideoStream.Height);
    }

    public static void ConversionSizeExceptionCheck(int width, int height)
    {
        if (height % 2 != 0 || width % 2 != 0)
        {
            throw new ArgumentException("FFMpeg yuv420p encoding requires the width and height to be a multiple of 2!");
        }
    }

    public static void ExtensionExceptionCheck(string filename, string extension)
    {
        if (!extension.Equals(Path.GetExtension(filename), StringComparison.OrdinalIgnoreCase))
        {
            throw new FFMpegException(FFMpegExceptionType.File,
                $"Invalid output file. File extension should be '{extension}' required.");
        }
    }

    public static void RootExceptionCheck()
    {
        if (string.IsNullOrWhiteSpace(GlobalFFOptions.Current.BinaryFolder))
        {
            throw new FFOptionsException("FFMpeg root is not configured in app config. Missing key 'BinaryFolder'.");
        }
    }

    public static void VerifyFFMpegExists(FFOptions ffMpegOptions)
    {
        if (_ffmpegVerified)
        {
            return;
        }

        var result = Instance.Finish(GlobalFFOptions.GetFFMpegBinaryPath(ffMpegOptions), "-version");

        VerifyResult(result);
    }

    public static async Task VerifyFFMpegExistsAsync(FFOptions ffMpegOptions, CancellationToken cancellationToken = default)
    {
        if (_ffmpegVerified)
        {
            return;
        }

        var ffmpegPath = await GlobalFFOptions.GetFFMpegBinaryPathAsync(ffMpegOptions, cancellationToken).ConfigureAwait(false);

        var result = await Instance.FinishAsync(ffmpegPath, "-version", cancellationToken).ConfigureAwait(false);

        VerifyResult(result);
    }

    private static void VerifyResult(IProcessResult result)
    {
        lock (_syncObject)
        {
            _ffmpegVerified = result.ExitCode is 0;
        }

        if (!_ffmpegVerified)
        {
            throw new FFMpegException(FFMpegExceptionType.Operation, "ffmpeg was not found on your system");
        }
    }
}
