using System;

namespace FFMpegCore.Models
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

        public FFMpegException(FFMpegExceptionType type, string? message = null, Exception? innerException = null, string ffMpegErrorOutput = "")
            : base(message, innerException)
        {
            FFMpegErrorOutput = ffMpegErrorOutput;
            Type = type;
        }

        public FFMpegExceptionType Type { get; }
        public string FFMpegErrorOutput { get; }
    }
}