using System;
using System.Drawing;
using System.IO;
using FFMpegCore.Exceptions;

namespace FFMpegCore.Helpers
{
    public static class FFMpegHelper
    {
        public static void ConversionSizeExceptionCheck(Image image)
        {
            ConversionSizeExceptionCheck(image.Size);
        }

        public static void ConversionSizeExceptionCheck(MediaAnalysis info)
        {
            ConversionSizeExceptionCheck(new Size(info.PrimaryVideoStream.Width, info.PrimaryVideoStream.Height));
        }

        public static void ConversionSizeExceptionCheck(Size size)
        {
            if (
                size.Height % 2 != 0 ||
                size.Width % 2 != 0
                )
            {
                throw new ArgumentException("FFMpeg yuv420p encoding requires the width and height to be a multiple of 2!");
            }
        }

        public static void ExtensionExceptionCheck(string filename, string extension)
        {
            if (!extension.Equals(Path.GetExtension(filename), StringComparison.OrdinalIgnoreCase))
                throw new FFMpegException(FFMpegExceptionType.File,
                    $"Invalid output file. File extension should be '{extension}' required.");
        }

        public static void RootExceptionCheck(string root)
        {
            if (root == null)
                throw new FFMpegException(FFMpegExceptionType.Dependency,
                    "FFMpeg root is not configured in app config. Missing key 'ffmpegRoot'.");
        }
    }
}
