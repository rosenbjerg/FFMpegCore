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
          
        }
    }
}
