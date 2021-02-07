using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FFMpegCore
{
    internal class MediaAnalysis : IMediaAnalysis
    {
        private static readonly Regex DurationRegex = new Regex(@"^(\d+):(\d{1,2}):(\d{1,2})\.(\d{1,3})", RegexOptions.Compiled);

        internal MediaAnalysis(string path, FFProbeAnalysis analysis)
        {
            Format = ParseFormat(analysis.Format);
            VideoStreams = analysis.Streams.Where(stream => stream.CodecType == "video").Select(ParseVideoStream).ToList();
            AudioStreams = analysis.Streams.Where(stream => stream.CodecType == "audio").Select(ParseAudioStream).ToList();
            PrimaryVideoStream = VideoStreams.OrderBy(stream => stream.Index).FirstOrDefault();
            PrimaryAudioStream = AudioStreams.OrderBy(stream => stream.Index).FirstOrDefault();
            Path = path;
        }

        private MediaFormat ParseFormat(Format analysisFormat)
        {
            return new MediaFormat
            {
                Duration = ParseDuration(analysisFormat.Duration),
                FormatName = analysisFormat.FormatName,
                FormatLongName = analysisFormat.FormatLongName,
                StreamCount = analysisFormat.NbStreams,
                ProbeScore = analysisFormat.ProbeScore,
                BitRate = long.Parse(analysisFormat.BitRate ?? "0"),
                Tags = analysisFormat.Tags,
            };
        }

        public string Path { get; }
        public string Extension => System.IO.Path.GetExtension(Path);

        public TimeSpan Duration => new[]
        {
            Format.Duration,
            PrimaryVideoStream?.Duration ?? TimeSpan.Zero,
            PrimaryAudioStream?.Duration ?? TimeSpan.Zero
        }.Max();

        public MediaFormat Format { get; }
        public AudioStream PrimaryAudioStream { get; }

        public VideoStream PrimaryVideoStream { get; }

        public List<VideoStream> VideoStreams { get; }
        public List<AudioStream> AudioStreams { get; }

        private VideoStream ParseVideoStream(FFProbeStream stream)
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
                Height = stream.Height ?? 0,
                Width = stream.Width ?? 0,
                Profile = stream.Profile,
                PixelFormat = stream.PixelFormat,
                Rotation = (int)float.Parse(stream.GetRotate() ?? "0"),
                Language = stream.GetLanguage(),
                Tags = stream.Tags,
            };
        }

        internal static TimeSpan ParseDuration(string duration)
        {
            if (!string.IsNullOrEmpty(duration))
            {
                var match = DurationRegex.Match(duration);
                if (match.Success)
                {
                    // ffmpeg may provide < 3-digit number of milliseconds (omitting trailing zeros), which won't simply parse correctly
                    // e.g. 00:12:02.11 -> 12 minutes 2 seconds and 110 milliseconds
                    var millisecondsPart = match.Groups[4].Value;
                    if (millisecondsPart.Length < 3)
                    {
                        millisecondsPart = millisecondsPart.PadRight(3, '0');
                    }

                    var hours = int.Parse(match.Groups[1].Value);
                    var minutes = int.Parse(match.Groups[2].Value);
                    var seconds = int.Parse(match.Groups[3].Value);
                    var milliseconds = int.Parse(millisecondsPart);
                    return new TimeSpan(0, hours, minutes, seconds, milliseconds);
                }
                else
                {
                    return TimeSpan.Zero;
                }
            }
            else
            {
                return TimeSpan.Zero;
            }
        }

        internal static TimeSpan ParseDuration(FFProbeStream ffProbeStream)
        {
            return ParseDuration(ffProbeStream.Duration);
        }

        private AudioStream ParseAudioStream(FFProbeStream stream)
        {
            return new AudioStream
            {
                Index = stream.Index,
                BitRate = !string.IsNullOrEmpty(stream.BitRate) ? ParseIntInvariant(stream.BitRate) : default,
                CodecName = stream.CodecName,
                CodecLongName = stream.CodecLongName,
                Channels = stream.Channels ?? default,
                ChannelLayout = stream.ChannelLayout,
                Duration = ParseDuration(stream),
                SampleRateHz = !string.IsNullOrEmpty(stream.SampleRate) ? ParseIntInvariant(stream.SampleRate) : default,
                Profile = stream.Profile,
                Language = stream.GetLanguage(),
                Tags = stream.Tags,
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