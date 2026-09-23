using System.Collections.Concurrent;
using FFMpegCore.Exceptions;
using Instances.Exceptions;

namespace FFMpegCore.Helpers;

public static class FFProbeHelper
{
    private static readonly ConcurrentDictionary<string, bool> VerifiedBinaries = new();

    public static void VerifyFFProbeExists(FFOptions ffMpegOptions)
    {
        var binaryPath = GlobalFFOptions.GetFFProbeBinaryPath(ffMpegOptions);
        if (VerifiedBinaries.ContainsKey(binaryPath))
        {
            return;
        }

        try
        {
            if (ProcessHelper.Run(binaryPath, "-version").ExitCode != 0)
            {
                throw new FFProbeException(FFMpegExceptionType.Operation, NotFoundMessage(binaryPath));
            }
        }
        catch (InstanceFileNotFoundException exception)
        {
            throw new FFProbeException(FFMpegExceptionType.Operation, NotFoundMessage(binaryPath), exception);
        }

        VerifiedBinaries[binaryPath] = true;
    }

    private static string NotFoundMessage(string binaryPath)
    {
        return $"ffprobe was not found on your system (tried \"{binaryPath}\")";
    }
}
