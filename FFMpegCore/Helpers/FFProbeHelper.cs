using FFMpegCore.Exceptions;
using Instances;

namespace FFMpegCore.Helpers;

public static class FFProbeHelper
{
    private static bool _ffprobeVerified;
    private static readonly object _syncObject = new();

    public static void RootExceptionCheck()
    {
        if (string.IsNullOrWhiteSpace(GlobalFFOptions.Current.BinaryFolder))
        {
            throw new FFOptionsException("FFProbe root is not configured in app config. Missing key 'BinaryFolder'.");
        }
    }

    public static void VerifyFFProbeExists(FFOptions ffOptions)
    {
        if (_ffprobeVerified)
        {
            return;
        }

        var result = Instance.Finish(GlobalFFOptions.GetFFProbeBinaryPath(ffOptions), "-version");

        VerifyResult(result);
    }

    public static async Task VerifyFFProbeExistsAsync(FFOptions ffOptions, CancellationToken cancellationToken = default)
    {
        if (_ffprobeVerified)
        {
            return;
        }

        var ffProbePath = await GlobalFFOptions.GetFFProbeBinaryPathAsync(ffOptions, cancellationToken).ConfigureAwait(false);

        var result = await Instance.FinishAsync(ffProbePath, "-version", cancellationToken).ConfigureAwait(false);

        VerifyResult(result);
    }

    private static void VerifyResult(IProcessResult result)
    {
        lock (_syncObject)
        {
            _ffprobeVerified = result.ExitCode is 0;
        }

        if (!_ffprobeVerified)
        {
            throw new FFProbeException("ffprobe was not found on your system");
        }
    }
}
