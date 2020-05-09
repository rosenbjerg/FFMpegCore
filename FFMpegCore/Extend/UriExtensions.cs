using System;

namespace FFMpegCore.Extend
{
    public static class UriExtensions
    {
        public static bool SaveStream(this Uri uri, string output)
        {
            return FFMpeg.SaveM3U8Stream(uri, output);
        }
    }
}