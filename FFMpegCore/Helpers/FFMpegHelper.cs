using System;
using System.Drawing;
using System.IO;
using FFMpegCore.Exceptions;
using Instances;

namespace FFMpegCore.Helpers
{
    public static class FFMpegHelper
    {
        private static bool _ffmpegVerified;

        public static void ConversionSizeExceptionCheck(Image image)
            => ConversionSizeExceptionCheck(image.Size.Width, image.Size.Height);

        public static void ConversionSizeExceptionCheck(IMediaAnalysis info)
            => ConversionSizeExceptionCheck(info.PrimaryVideoStream!.Width, info.PrimaryVideoStream.Height);

        private static void ConversionSizeExceptionCheck(int width, int height)
        {
            if (height % 2 != 0 || width % 2 != 0 )
                throw new ArgumentException("FFMpeg yuv420p encoding requires the width and height to be a multiple of 2!");
        }

        public static void ExtensionExceptionCheck(string filename, string extension)
        {
            if (!extension.Equals(Path.GetExtension(filename), StringComparison.OrdinalIgnoreCase))
                throw new FFMpegException(FFMpegExceptionType.File,
                    $"Invalid output file. File extension should be '{extension}' required.");
        }

        public static void RootExceptionCheck()
        {
            if (FFMpegOptions.Options.RootDirectory == null)
                throw new FFMpegException(FFMpegExceptionType.Dependency,
                    "FFMpeg root is not configured in app config. Missing key 'ffmpegRoot'.");
        }
        
        public static void VerifyFFMpegExists()
        {
            if (_ffmpegVerified) return;
            var (exitCode, _) = Instance.Finish(FFMpegOptions.Options.FFMpegBinary(), "-version");
            _ffmpegVerified = exitCode == 0;
            if (!_ffmpegVerified) throw new FFMpegException(FFMpegExceptionType.Operation, "ffmpeg was not found on your system");
        }
    }
}
