using FFMpegCore.Exceptions;
using Instances;

namespace FFMpegCore.Helpers
{
    public static class FFMpegHelper
    {
        private static string? _ffmpegPathVerified;

        public static void ConversionSizeExceptionCheck(IMediaAnalysis info)
            => ConversionSizeExceptionCheck(info.PrimaryVideoStream!.Width, info.PrimaryVideoStream.Height);

        public static void ConversionSizeExceptionCheck(int width, int height)
        {
            if (height % 2 != 0 || width % 2 != 0)
            {
                throw new ArgumentException("FFMpeg yuv420p encoding requires the width and height to be a multiple of 2!");
            }
        }

        public static void ExtensionExceptionCheck(string filename, string extension)
        {
            if (!extension.Equals(Path.GetExtension(filename), StringComparison.OrdinalIgnoreCase))
            {
                throw new FFMpegException(FFMpegExceptionType.File,
                    $"Invalid output file. File extension should be '{extension}' required.");
            }
        }

        public static void RootExceptionCheck()
        {
            if (GlobalFFOptions.Current.BinaryFolder == null)
            {
                throw new FFOptionsException("FFMpeg root is not configured in app config. Missing key 'BinaryFolder'.");
            }
        }

        public static void VerifyFFMpegExists(FFOptions ffMpegOptions)
        {
            var binaryPath = GlobalFFOptions.GetFFMpegBinaryPath(ffMpegOptions);
            if (_ffmpegPathVerified != null && _ffmpegPathVerified == binaryPath)
            {
                return;
            }

            try
            {
                var result = Instance.Finish(binaryPath, "-version");
                _ffmpegPathVerified = result.ExitCode == 0 ? binaryPath : null;
            }
            catch (Exception e)
            {
                throw new FFMpegException(FFMpegExceptionType.Operation, "ffmpeg was not found on your system", e);
            }

            if (_ffmpegPathVerified == null)
            {
                throw new FFMpegException(FFMpegExceptionType.Operation, "ffmpeg was not found on your system");
            }
        }
    }
}
