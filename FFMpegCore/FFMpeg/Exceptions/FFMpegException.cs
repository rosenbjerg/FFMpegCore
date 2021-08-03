using System;

namespace FFMpegCore.Exceptions
{
    public enum FFMpegExceptionType
    {
        Conversion,
        File,
        Operation,
        Process
    }

    public class FFMpegException : Exception
    {
        public FFMpegException(FFMpegExceptionType type, string message, Exception? innerException = null, string ffMpegErrorOutput = "")
            : base(message, innerException)
        {
            FFMpegErrorOutput = ffMpegErrorOutput;
            Type = type;
        }
        public FFMpegException(FFMpegExceptionType type, string message, string ffMpegErrorOutput = "")
            : base(message)
        {
            FFMpegErrorOutput = ffMpegErrorOutput;
            Type = type;
        }
        public FFMpegException(FFMpegExceptionType type, string message)
            : base(message)
        {
            FFMpegErrorOutput = string.Empty;
            Type = type;
        }
        
        public FFMpegExceptionType Type { get; }
        public string FFMpegErrorOutput { get; }
    }
    public class FFOptionsException : Exception
    {
        public FFOptionsException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }

    public class FFMpegArgumentException : Exception
    {
        public FFMpegArgumentException(string? message = null, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }

    public class FFMpegStreamFormatException : FFMpegException
    {
        public FFMpegStreamFormatException(FFMpegExceptionType type, string message, Exception? innerException = null)
            : base(type, message, innerException)
        {
        }
    }
}