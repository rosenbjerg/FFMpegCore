using System.Collections.Concurrent;
using FFMpegCore.Exceptions;
using Instances.Exceptions;

namespace FFMpegCore.Helpers;

public static class FFMpegHelper
{
    private static readonly ConcurrentDictionary<string, bool> _verifiedBinaries = new();

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

    public static void VerifyFFMpegExists(FFOptions ffMpegOptions)
    {
        var binaryPath = GlobalFFOptions.GetFFMpegBinaryPath(ffMpegOptions);
        if (_verifiedBinaries.ContainsKey(binaryPath))
        {
            return;
        }

        try
        {
            if (ProcessHelper.Run(binaryPath, "-version").ExitCode != 0)
            {
                throw new FFMpegException(FFMpegExceptionType.Operation, NotFoundMessage(binaryPath));
            }
        }
        catch (InstanceFileNotFoundException exception)
        {
            throw new FFMpegException(FFMpegExceptionType.Operation, NotFoundMessage(binaryPath), exception);
        }

        _verifiedBinaries[binaryPath] = true;
    }

    private static string NotFoundMessage(string binaryPath)
    {
        return $"ffmpeg was not found on your system (tried \"{binaryPath}\")";
    }
}
