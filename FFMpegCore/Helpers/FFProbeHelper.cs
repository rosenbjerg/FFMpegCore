using FFMpegCore.Exceptions;

namespace FFMpegCore.Helpers;

public static class FFProbeHelper
{
    private static bool _ffprobeVerified;

    public static void RootExceptionCheck()
    {
        if (GlobalFFOptions.Current.BinaryFolder == null)
        {
            throw new FFOptionsException("FFProbe root is not configured in app config. Missing key 'BinaryFolder'.");
        }
    }

    public static void VerifyFFProbeExists(FFOptions ffMpegOptions)
    {
        if (_ffprobeVerified)
        {
            return;
        }

        var result = ProcessHelper.Run(GlobalFFOptions.GetFFProbeBinaryPath(ffMpegOptions), "-version");
        _ffprobeVerified = result.ExitCode == 0;
        if (!_ffprobeVerified)
        {
            throw new FFProbeException("ffprobe was not found on your system");
        }
    }
}
