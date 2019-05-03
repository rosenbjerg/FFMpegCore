using System;
using System.IO;
using System.Runtime.InteropServices;
using FFMpegCore.FFMPEG.Exceptions;

namespace FFMpegCore.Helpers
{
    public class FFProbeHelper
    {
        public static int Gcd(int first, int second)
        {
            while (first != 0 && second != 0)
            {
                if (first > second)
                    first -= second;
                else second -= first;
            }
            return first == 0 ? second : first;
        }

        public static void RootExceptionCheck(string root)
        {
            if (root == null)
                throw new FFMpegException(FFMpegExceptionType.Dependency,
                    "FFProbe root is not configured in app config. Missing key 'ffmpegRoot'.");

            var progName = "ffprobe";
            if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
                var target = Environment.Is64BitProcess ? "x64" : "x86";

                progName = $"{target}{Path.DirectorySeparatorChar}{progName}.exe";
            }

            var path = root + $"{Path.DirectorySeparatorChar}{progName}";

            if (!File.Exists(path))
                throw new FFMpegException(FFMpegExceptionType.Dependency,
                    $"FFProbe cannot be found in the in {path}...");
        }
    }
}
