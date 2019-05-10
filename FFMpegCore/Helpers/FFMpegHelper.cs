using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using FFMpegCore.FFMPEG.Exceptions;

namespace FFMpegCore.Helpers
{
    public static class FFMpegHelper
    {
        public static void ConversionSizeExceptionCheck(Image image)
        {
            ConversionSizeExceptionCheck(image.Size);
        }

        public static void ConversionSizeExceptionCheck(VideoInfo info)
        {
            ConversionSizeExceptionCheck(new Size(info.Width, info.Height));
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

        public static void OutputExistsExceptionCheck(FileInfo output)
        {
            if (File.Exists(output.FullName))
            {
                throw new FFMpegException(FFMpegExceptionType.File,
                    $"The output file: {output} already exists!");
            }
        }

        public static void InputExistsExceptionCheck(FileInfo input)
        {
            if (!File.Exists(input.FullName))
            {
                throw new FFMpegException(FFMpegExceptionType.File,
                    $"Input {input.FullName} does not exist!");
            }
        }

        public static void ConversionExceptionCheck(FileInfo originalVideo, FileInfo convertedPath)
        {
            OutputExistsExceptionCheck(convertedPath);
            InputExistsExceptionCheck(originalVideo);
        }

        public static void InputsExistExceptionCheck(params FileInfo[] paths)
        {
            foreach (var path in paths)
            {
                InputExistsExceptionCheck(path);
            }
        }

        public static void ExtensionExceptionCheck(FileInfo output, string expected)
        {
            if (!expected.Equals(new FileInfo(output.FullName).Extension, StringComparison.OrdinalIgnoreCase))
                throw new FFMpegException(FFMpegExceptionType.File,
                    $"Invalid output file. File extension should be '{expected}' required.");
        }

        public static void RootExceptionCheck(string root)
        {
            if (root == null)
                throw new FFMpegException(FFMpegExceptionType.Dependency,
                    "FFMpeg root is not configured in app config. Missing key 'ffmpegRoot'.");
        }
    }
}
