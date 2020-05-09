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
        public FFMpegException(FFMpegExceptionType type, string message): this(type, message, null) { }

        public FFMpegException(FFMpegExceptionType type, string? message = null, Exception? innerException = null)
            : base(message, innerException)
        {
            Type = type;
        }

        public FFMpegExceptionType Type { get; }
    }
}