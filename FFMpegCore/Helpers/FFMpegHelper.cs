using System.Collections.Concurrent;
using System.Drawing;
using FFMpegCore.Exceptions;
using Instances.Exceptions;

namespace FFMpegCore.Helpers;

internal static class FFMpegHelper
{
    private static readonly ConcurrentDictionary<string, bool> _verifiedBinaries = new();

    public static Size FrameSizeOf(IMediaAnalysis analysis, string paramName)
    {
        var stream = analysis.PrimaryVideoStream;
        if (stream?.Width is not { } width || stream.Height is not { } height)
        {
            throw new ArgumentException($"ffprobe reported no video frame size for {analysis.Path ?? "the input"}", paramName);
        }

        return new Size(width, height);
    }

    public static void ConversionSizeExceptionCheck(Size size)
    {
        if (size.Height % 2 != 0 || size.Width % 2 != 0)
        {
            throw new ArgumentException("FFMpeg yuv420p encoding requires the width and height to be a multiple of 2!");
        }
    }

    public static void VerifyFFMpegExists(FFOptions ffOptions)
    {
        var binaryPath = GlobalFFOptions.GetFFMpegBinaryPath(ffOptions);
        if (_verifiedBinaries.ContainsKey(binaryPath))
        {
            return;
        }

        try
        {
            if (ProcessHelper.Run(binaryPath, "-version").ExitCode != 0)
            {
                throw new FFMpegException(NotFoundMessage(binaryPath));
            }
        }
        catch (InstanceFileNotFoundException exception)
        {
            throw new FFMpegException(NotFoundMessage(binaryPath), exception);
        }

        _verifiedBinaries[binaryPath] = true;
    }

    private static string NotFoundMessage(string binaryPath)
    {
        return $"ffmpeg was not found on your system (tried \"{binaryPath}\")";
    }
}
