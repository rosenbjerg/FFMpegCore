using System;
using System.IO;
using FFMpegCore.FFMPEG;

namespace FFMpegCore.Extend
{
    public static class UriExtensions
    {
        public static VideoInfo SaveStream(this Uri uri, FileInfo output)
        {
            return new FFMpeg().SaveM3U8Stream(uri, output);
        }
    }
}