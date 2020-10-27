﻿using FFMpegCore.Exceptions;
using Instances;

namespace FFMpegCore.Helpers
{
    public class FFProbeHelper
    {
        private static bool _ffprobeVerified;

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

        public static void RootExceptionCheck()
        {
            if (FFMpegOptions.Options.RootDirectory == null)
                throw new FFMpegException(FFMpegExceptionType.Dependency,
                    "FFProbe root is not configured in app config. Missing key 'ffmpegRoot'.");
            
        }
        
        public static void VerifyFFProbeExists()
        {
            if (_ffprobeVerified) return;
            var (exitCode, _) = Instance.Finish(FFMpegOptions.Options.FFProbeBinary(), "-version");
            _ffprobeVerified = exitCode == 0;
            if (!_ffprobeVerified) throw new FFMpegException(FFMpegExceptionType.Operation, "ffprobe was not found on your system");
        }
    }
}
