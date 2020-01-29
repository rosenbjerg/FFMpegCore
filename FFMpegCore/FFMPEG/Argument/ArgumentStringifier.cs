using FFMpegCore.FFMPEG.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace FFMpegCore.FFMPEG.Argument
{
    internal static class ArgumentStringifier
    {
        internal static string Speed(Speed speed)
        {
            return $"-preset {speed.ToString().ToLower()} ";
        }

        internal static string Speed(int cpu)
        {
            return $"-quality good -cpu-used {cpu} -deadline realtime ";
        }

        internal static string Audio(AudioCodec codec, AudioQuality bitrate)
        {
            return Audio(codec) + Audio(bitrate);
        }

        internal static string Audio(AudioCodec codec, int bitrate)
        {
            return Audio(codec) + Audio(bitrate);
        }

        internal static string Audio(AudioCodec codec)
        {
            return $"-codec:a {codec.ToString().ToLower()} ";
        }

        internal static string Audio(AudioQuality bitrate)
        {
            return Audio((int)bitrate);
        }

        internal static string Audio(int bitrate)
        {
            return $"-b:a {bitrate}k -strict experimental ";
        }

        internal static string Video(VideoCodec codec, int bitrate = 0)
        {
            var video = $"-codec:v {codec.ToString().ToLower()} -pix_fmt yuv420p ";

            if (bitrate > 0)
            {
                video += $"-b:v {bitrate}k ";
            }

            return video;
        }

        internal static string Threads(bool multiThread)
        {
            var threadCount = multiThread
                ? Environment.ProcessorCount
                : 1;

            return Threads(threadCount);
        }

        internal static string Threads(int threads)
        {        
            return $"-threads {threads} ";
        }

        internal static string Input(Uri uri)
        {
            return Input(uri.AbsolutePath);
        }

        internal static string Disable(Channel type)
        {
            switch (type)
            {
                case Channel.Video:
                    return "-vn ";
                case Channel.Audio:
                    return "-an ";
                default:
                    return string.Empty;
            }
        }

        internal static string Input(VideoInfo input)
        {
            return $"-i \"{input.FullName}\" ";
        }

        internal static string Input(FileInfo input)
        {
            return $"-i \"{input.FullName}\" ";
        }

        internal static string Output(FileInfo output)
        {
            return $"\"{output.FullName}\"";
        }

        internal static string Output(string output)
        {
            return $"\"{output}\"";
        }

        internal static string Input(string template)
        {
            return $"-i \"{template}\" ";
        }

        internal static string Scale(VideoSize size, int width =-1)
        {
            return size == VideoSize.Original ? string.Empty : Scale(width, (int)size);
        }

        internal static string Scale(int width, int height)
        {
            return $"-vf scale={width}:{height} ";
        }

        internal static string Scale(Size size)
        {
            return Scale(size.Width, size.Height);
        }

        internal static string Size(Size? size)
        {
            if (!size.HasValue) return string.Empty;

            var formatedSize = $"{size.Value.Width}x{size.Value.Height}";

            return $"-s {formatedSize} ";
        }

        internal static string ForceFormat(VideoCodec codec)
        {
            return $"-f {codec.ToString().ToLower()} ";
        }

        internal static string BitStreamFilter(Channel type, Filter filter)
        {
            switch (type)
            {
                case Channel.Audio:
                    return $"-bsf:a {filter.ToString().ToLower()} ";
                case Channel.Video:
                    return $"-bsf:v {filter.ToString().ToLower()} ";
                default:
                    return string.Empty;
            }
        }

        internal static string Copy(Channel type = Channel.Both)
        {
            switch (type)
            {
                case Channel.Audio:
                    return "-c:a copy ";
                case Channel.Video:
                    return "-c:v copy ";
                default:
                    return "-c copy ";
            }
        }

        internal static string Seek(TimeSpan? seek)
        {
            return !seek.HasValue ? string.Empty : $"-ss {seek} ";
        }

        internal static string FrameOutputCount(int number)
        {
            return $"-vframes {number} ";
        }

        internal static string Loop(int count)
        {
            return $"-loop {count} ";
        }

        internal static string FinalizeAtShortestInput(bool applicable)
        {
            return applicable ? "-shortest " : string.Empty;
        }

        internal static string InputConcat(IEnumerable<string> paths)
        {
            return $"-i \"concat:{string.Join(@"|", paths)}\" ";
        }

        internal static string FrameRate(double frameRate)
        {
            return $"-r {frameRate} ";
        }

        internal static string StartNumber(int v)
        {
            return $"-start_number {v} ";
        }

        internal static string Duration(TimeSpan? duration)
        {
            return !duration.HasValue ? string.Empty : $"-t {duration} ";
        }
    }
}