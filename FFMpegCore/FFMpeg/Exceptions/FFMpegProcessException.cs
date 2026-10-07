namespace FFMpegCore.Exceptions;

public class FFMpegProcessException : FFMpegException
{
    public FFMpegProcessException(FFMpegResult result)
        : base(FFMpegExceptionType.Process, $"ffmpeg exited with non-zero exit-code ({result.ExitCode} - {string.Join("\n", result.StandardError)})")
    {
        Result = result;
    }

    public FFMpegResult Result { get; }
}
