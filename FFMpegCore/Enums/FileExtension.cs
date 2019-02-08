using FFMpegCore.FFMPEG.Enums;
using System;

namespace FFMpegCore.Enums
{
    public static class FileExtension
    {
        public static string ForType(VideoType type)
        {
            switch (type)
            {
                case VideoType.Mp4: return Mp4;
                case VideoType.Ogv: return Ogv;
                case VideoType.Ts: return Ts;
                case VideoType.WebM: return WebM;
                default: throw new Exception("The extension for this video type is not defined.");
            }
        }
        public static string ForCodec(VideoCodec type)
        {
            switch (type)
            {
                case VideoCodec.LibX264: return Mp4;
                case VideoCodec.LibVpx: return WebM;
                case VideoCodec.LibTheora: return Ogv;
                case VideoCodec.MpegTs: return Ts;
                case VideoCodec.Png: return Png;
                default: throw new Exception("The extension for this video type is not defined.");
            }
        }
        public static readonly string Mp4 = ".mp4";
        public static readonly string Mp3 = ".mp3";
        public static readonly string Ts = ".ts";
        public static readonly string Ogv = ".ogv";
        public static readonly string Png = ".png";
        public static readonly string WebM = ".webm";
    }
}
