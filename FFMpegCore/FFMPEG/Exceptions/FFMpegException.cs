using System;
using System.Text;

namespace FFMpegCore.FFMPEG.Exceptions
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
        public FFMpegException(FFMpegExceptionType type): this(type, null, null) { }

        public FFMpegException(FFMpegExceptionType type, StringBuilder sb): this(type, sb.ToString(), null) { }

        public FFMpegException(FFMpegExceptionType type, string message): this(type, message, null) { }

        public FFMpegException(FFMpegExceptionType type, string message, FFMpegException innerException)
            : base(message, innerException)
        {
            Type = type;
        }

        public FFMpegExceptionType Type { get; set; }
    }
}