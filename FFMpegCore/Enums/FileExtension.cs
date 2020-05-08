using System;
using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.Enums
{
    public static class FileExtension
    {
        public static string ForType(VideoType type)
        {
            return type switch
            {
                VideoType.Mp4 => Mp4,
                VideoType.Ogv => Ogv,
                VideoType.Ts => Ts,
                VideoType.WebM => WebM,
                _ => throw new Exception("The extension for this video type is not defined.")
            };
        }
        public static string ForCodec(VideoCodec type)
        {
            return type switch
            {
                VideoCodec.LibX264 => Mp4,
                VideoCodec.LibVpx => WebM,
                VideoCodec.LibTheora => Ogv,
                VideoCodec.MpegTs => Ts,
                VideoCodec.Png => Png,
                _ => throw new Exception("The extension for this video type is not defined.")
            };
        }
        public static readonly string Mp4 = ".mp4";
        public static readonly string Mp3 = ".mp3";
        public static readonly string Ts = ".ts";
        public static readonly string Ogv = ".ogv";
        public static readonly string Png = ".png";
        public static readonly string WebM = ".webm";
    }
}
