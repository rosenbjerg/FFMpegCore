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

    public static class TestResources
    {
        public static readonly string Mp4Video = "./Resources/input_3sec.mp4";
        public static readonly string WebmVideo = "./Resources/input_3sec.webm";
        public static readonly string Mp4WithoutVideo = "./Resources/input_audio_only_10sec.mp4";
        public static readonly string Mp4WithoutAudio = "./Resources/input_video_only_3sec.mp4";
        public static readonly string RawAudio = "./Resources/audio.raw";
        public static readonly string Mp3Audio = "./Resources/audio.mp3";
        public static readonly string PngImage = "./Resources/cover.png";
        public static readonly string ImageCollection = "./Resources/images";
    }
}
