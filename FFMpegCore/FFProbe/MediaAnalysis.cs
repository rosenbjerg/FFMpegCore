using System;
using System.Collections.Generic;
using System.Linq;

namespace FFMpegCore
{
    public class MediaAnalysis
    {
	    private readonly TimeSpan? _mainDuration;

        internal MediaAnalysis(string path, FFProbeAnalysis analysis, TimeSpan? mainDuration)
        {
            VideoStreams = analysis.Streams.Where(stream => stream.CodecType == "video").Select(ParseVideoStream).ToList();
            AudioStreams = analysis.Streams.Where(stream => stream.CodecType == "audio").Select(ParseAudioStream).ToList();
            PrimaryVideoStream = VideoStreams.OrderBy(stream => stream.Index).FirstOrDefault();
            PrimaryAudioStream = AudioStreams.OrderBy(stream => stream.Index).FirstOrDefault();
            Path = path;

            _mainDuration = mainDuration;
        }


        public string Path { get; }
        public string Extension => System.IO.Path.GetExtension(Path);

        public TimeSpan Duration => TimeSpan.FromSeconds(
	        new [] { PrimaryVideoStream?.Duration.TotalSeconds ?? 0,
	            PrimaryAudioStream?.Duration.TotalSeconds ?? 0,
	            _mainDuration?.TotalSeconds ?? 0}
		        .Max());
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
                Language = stream.Tags?.Language
            };
        }

        private static TimeSpan ParseDuration(FFProbeStream ffProbeStream)
        {
            return ffProbeStream.Duration != null
                ? TimeSpan.FromSeconds(ParseDoubleInvariant(ffProbeStream.Duration))
                : TimeSpan.Parse(FixTimeSpanString(ffProbeStream.Tags?.Duration ?? "0"));
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
                Language = stream.Tags?.Language
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

        private static string FixTimeSpanString(string line)
        {
			const int maxFractionDigits = 7;
			var dotPosition = line.LastIndexOf('.');
			if (dotPosition < 0 || dotPosition == line.Length - 1)
			{
				return line;
			}
			
			var lastPart = line.Substring(dotPosition + 1);
            if(lastPart.Contains(':') || lastPart.Length <= maxFractionDigits)
            {
                return line;
            }

            return line.Substring(0, dotPosition + 1) + lastPart.Substring(0, maxFractionDigits);			
        }	        
    }
}