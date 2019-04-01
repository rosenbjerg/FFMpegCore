using System;
using System.IO;
using FFMpegCore.Enums;

namespace FFMpegCore.Test.Resources
{
    public enum AudioType
    {
        Mp3
    }

    public enum ImageType
    {
        Png
    }

    public static class VideoLibrary
    {
        public static readonly FileInfo LocalVideo = new FileInfo(".\\Resources\\input.mp4");
        public static readonly FileInfo LocalVideoAudioOnly = new FileInfo(".\\Resources\\audio_only.mp4");
        public static readonly FileInfo LocalVideoNoAudio = new FileInfo(".\\Resources\\mute.mp4");
        public static readonly FileInfo LocalAudio = new FileInfo(".\\Resources\\audio.mp3");
        public static readonly FileInfo LocalCover = new FileInfo(".\\Resources\\cover.png");
        public static readonly FileInfo ImageDirectory = new FileInfo(".\\Resources\\images");
        public static readonly FileInfo ImageJoinOutput = new FileInfo(".\\Resources\\images\\output.mp4");

        public static FileInfo OutputLocation(this FileInfo file, VideoType type)
        {
            return OutputLocation(file, type, "_converted");
        }

        public static FileInfo OutputLocation(this FileInfo file, AudioType type)
        {
            return OutputLocation(file, type, "_audio");
        }

        public static FileInfo OutputLocation(this FileInfo file, ImageType type)
        {
            return OutputLocation(file, type, "_screenshot");
        }

        public static FileInfo OutputLocation(this FileInfo file, Enum type, string keyword)
        {
            string originalLocation = file.Directory.FullName,
                outputFile = file.Name.Replace(file.Extension, keyword + "." + type.ToString().ToLower());

            return new FileInfo($"{originalLocation}\\{outputFile}");
        }
    }
}