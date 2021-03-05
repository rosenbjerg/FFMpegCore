using System;

namespace FFMpegCore.Exceptions
{
    public enum FFMpegExceptionType
    {
        Dependency,
        Conversion,
        File,
        Operation,
        Process
    }

    public abstract class FFException : Exception
    {
        protected FFException(string message) : base(message) { }
        protected FFException(string message, Exception innerException) : base(message, innerException) { }
    }
    public abstract class FFProcessException : FFException
    {
        protected FFProcessException(string process, int exitCode, string errorOutput)
            : base($"{process} exited with non-zero exit-code {exitCode}\n{errorOutput}")
        {
            ExitCode = exitCode;
            ErrorOutput = errorOutput;
        }

        public int ExitCode { get; }
        public string ErrorOutput { get; }
    }
    public class FFMpegProcessException : FFProcessException
    {
        public FFMpegProcessException(int exitCode, string errorOutput)
            : base("ffmpeg", exitCode, errorOutput) { }
    }
    public class FFProbeProcessException : FFProcessException
    {
        public FFProbeProcessException(int exitCode, string errorOutput)
            : base("ffprobe", exitCode, errorOutput) { }
    }

    public class FFMpegException : Exception
    {
        public FFMpegException(FFMpegExceptionType type, string? message = null, Exception? innerException = null, string ffMpegErrorOutput = "")
            : base(message, innerException)
        {
            FFMpegErrorOutput = ffMpegErrorOutput;
            Type = type;
        }

        public FFMpegExceptionType Type { get; }
        public string FFMpegErrorOutput { get; }
    }

    public class FFMpegArgumentException : Exception
    {
        public FFMpegArgumentException(string? message = null, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}