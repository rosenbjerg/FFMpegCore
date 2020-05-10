using System;
using System.Collections.Generic;
using System.Linq;

namespace FFMpegCore
{
    public class MediaAnalysis
    {
        internal MediaAnalysis(string path, FFProbeAnalysis analysis)
        {
            VideoStreams = analysis.Streams.Where(stream => stream.CodecType == "video").Select(ParseVideoStream).ToList();
            AudioStreams = analysis.Streams.Where(stream => stream.CodecType == "audio").Select(ParseAudioStream).ToList();
            PrimaryVideoStream = VideoStreams.OrderBy(stream => stream.Index).FirstOrDefault();
            PrimaryAudioStream = AudioStreams.OrderBy(stream => stream.Index).FirstOrDefault();
            Path = path;
        }


        public string Path { get; }
        public string Extension => System.IO.Path.GetExtension(Path);

        public TimeSpan Duration => TimeSpan.FromSeconds(Math.Max(
            PrimaryVideoStream?.Duration.TotalSeconds ?? 0,
            PrimaryAudioStream?.Duration.TotalSeconds ?? 0));
        public AudioStream PrimaryAudioStream { get; }

        public VideoStream PrimaryVideoStream { get; }

        public List<VideoStream> VideoStreams { get; }
        public List<AudioStream> AudioStreams { get; }

        private VideoStream ParseVideoStream(Stream stream)
        {
            return new VideoStream
            {
                Index = stream.Index,
                AvgFrameRate = DivideRatio(ParseRatioDouble(stream.AvgFrameRate, '/')),
                BitRate = !string.IsNullOrEmpty(stream.BitRate) ? ParseIntInvariant(stream.BitRate) : default,
                BitsPerRawSample = !string.IsNullOrEmpty(stream.BitsPerRawSample) ? ParseIntInvariant(stream.BitsPerRawSample) : default,
                CodecName = stream.CodecName,
                CodecLongName = stream.CodecLongName,
                DisplayAspectRatio = ParseRatioInt(stream.DisplayAspectRatio, ':'),
                Duration = ParseDuration(stream),
                FrameRate = DivideRatio(ParseRatioDouble(stream.FrameRate, '/')),
                Height = stream.Height!.Value,
                Width = stream.Width!.Value,
                Profile = stream.Profile,
                PixelFormat = stream.PixelFormat
            };
        }

        private static TimeSpan ParseDuration(Stream stream)
        {
            return stream.Duration != null
                ? TimeSpan.FromSeconds(ParseDoubleInvariant(stream.Duration))
                : TimeSpan.Parse(stream.Tags.Duration ?? "0");
        }

        private AudioStream ParseAudioStream(Stream stream)
        {
            return new AudioStream
            {
                Index = stream.Index,
                BitRate = !string.IsNullOrEmpty(stream.BitRate) ? ParseIntInvariant(stream.BitRate) : default,
                CodecName = stream.CodecName,
                CodecLongName = stream.CodecLongName,
                Channels = stream.Channels ?? default,
                ChannelLayout = stream.ChannelLayout,
                Duration = TimeSpan.FromSeconds(ParseDoubleInvariant(stream.Duration ?? stream.Tags.Duration ?? "0")),
                SampleRateHz = !string.IsNullOrEmpty(stream.SampleRate) ? ParseIntInvariant(stream.SampleRate) : default
            };
        }

        private static double DivideRatio((double, double) ratio) => ratio.Item1 / ratio.Item2;
        private static (int, int) ParseRatioInt(string input, char separator)
        {
            if (string.IsNullOrEmpty(input)) return (0, 0);
            var ratio = input.Split(separator);
            return (ParseIntInvariant(ratio[0]), ParseIntInvariant(ratio[1]));
        }
        private static (double, double) ParseRatioDouble(string input, char separator)
        {
            if (string.IsNullOrEmpty(input)) return (0, 0);
            var ratio = input.Split(separator);
            return (ratio.Length > 0 ? ParseDoubleInvariant(ratio[0]) : 0, ratio.Length > 1 ? ParseDoubleInvariant(ratio[1]) : 0);
        }

        private static double ParseDoubleInvariant(string line) =>
            double.Parse(line, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
        private static int ParseIntInvariant(string line) =>
            int.Parse(line, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
    }
}