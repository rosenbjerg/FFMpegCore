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

    public class FFMpegException : Exception
    {
        public FFMpegException(FFMpegExceptionType type, string? message = null, Exception? innerException = null, string ffmpegErrorOutput = "", string ffmpegOutput = "")
            : base(message, innerException)
        {
            FfmpegOutput = ffmpegOutput;
            FfmpegErrorOutput = ffmpegErrorOutput;
            Type = type;
        }

        public FFMpegExceptionType Type { get; }
        public string FfmpegOutput { get; }
        public string FfmpegErrorOutput { get; }
    }
}