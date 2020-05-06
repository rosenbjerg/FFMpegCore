using FFMpegCore.FFMPEG.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace FFMpegCore.FFMPEG.Argument.Fluent
{
    public static class ArgumentContainerFluentExtensions
    {
        public static ArgumentContainer AudioCodec(this ArgumentContainer container, AudioCodec codec)
        {
            container.Add(new AudioCodecArgument(codec));
            return container;
        }

        public static ArgumentContainer AudioBitrate(this ArgumentContainer container, AudioQuality audioQuality)
        {
            container.Add(new AudioBitrateArgument(audioQuality));
            return container;
        }
        public static ArgumentContainer AudioBitrate(this ArgumentContainer container, int bitrate)
        {
            container.Add(new AudioBitrateArgument(bitrate));
            return container;
        }

        public static ArgumentContainer AudioSamplingRate(this ArgumentContainer container)
        {
            container.Add(new AudioSamplingRateArgument());
            return container;
        }

        public static ArgumentContainer AudioSamplingRate(this ArgumentContainer container, int sampleRate)
        {
            container.Add(new AudioSamplingRateArgument(sampleRate));
            return container;
        }

        public static ArgumentContainer BitStreamFilter(this ArgumentContainer container, Channel first, Filter second)
        {
            container.Add(new BitStreamFilterArgument(first, second));
            return container;
        }

        public static ArgumentContainer Concat(this ArgumentContainer container, IEnumerable<string> paths)
        {
            container.Add(new ConcatArgument(paths));
            return container;
        }

        public static ArgumentContainer ConstantRateFactor(this ArgumentContainer container, int crf)
        {
            container.Add(new ConstantRateFactorArgument(crf));
            return container;
        }

        public static ArgumentContainer Copy(this ArgumentContainer container)
        {
            container.Add(new CopyArgument());
            return container;
        }

        public static ArgumentContainer Copy(this ArgumentContainer container, Channel value)
        {
            container.Add(new CopyArgument(value));
            return container;
        }

        public static ArgumentContainer CpuSpeed(this ArgumentContainer container, int value)
        {
            container.Add(new CpuSpeedArgument(value));
            return container;
        }

        public static ArgumentContainer DisableChannel(this ArgumentContainer container, Channel channel)
        {
            container.Add(new DisableChannelArgument(channel));
            return container;
        }

        public class DrawTextOptions
        {
            public string Text { get; set; }
            public string FontPath { get; set; }
            public List<(string, string)> Params { get; private set; }

            public DrawTextOptions()
            {
                Params = new List<(string, string)>();
            }

            public DrawTextOptions AddParam(string key, string value)
            {
                Params.Add((key, value));
                return this;
            }
        }

        public static ArgumentContainer DrawText(this ArgumentContainer container, Action<DrawTextOptions> builder)
        {
            var argumentParams = new DrawTextOptions();
            builder.Invoke(argumentParams);
            container.Add(new DrawTextArgument(argumentParams.Text, argumentParams.FontPath, argumentParams.Params.ToArray()));
            return container;
        }

        public static ArgumentContainer DrawText(this ArgumentContainer container, string text, string fontPath, params (string, string)[] optionalArguments)
        {
            container.Add(new DrawTextArgument(text, fontPath, optionalArguments));
            return container;
        }

        public static ArgumentContainer Duration(this ArgumentContainer container, TimeSpan? duration)
        {
            container.Add(new DurationArgument(duration));
            return container;
        }

        public static ArgumentContainer FastStart(this ArgumentContainer container)
        {
            container.Add(new FaststartArgument());
            return container;
        }

        public static ArgumentContainer ForceFormat(this ArgumentContainer container, VideoCodec codec)
        {
            container.Add(new ForceFormatArgument(codec));
            return container;
        }

        public static ArgumentContainer FrameOutputCount(this ArgumentContainer container, int count)
        {
            container.Add(new FrameOutputCountArgument(count));
            return container;
        }

        public static ArgumentContainer FrameRate(this ArgumentContainer container, double framerate)
        {
            container.Add(new FrameRateArgument(framerate));
            return container;
        }

        public static ArgumentContainer Input(this ArgumentContainer container, string path)
        {
            container.Add(new InputArgument(path));
            return container;
        }

        public static ArgumentContainer Input(this ArgumentContainer container, IEnumerable<string> paths)
        {
            container.Add(new InputArgument(paths.ToArray()));
            return container;
        }

        public static ArgumentContainer Input(this ArgumentContainer container, params string[] paths)
        {
            container.Add(new InputArgument(paths));
            return container;
        }

        public static ArgumentContainer Input(this ArgumentContainer container, VideoInfo path)
        {
            container.Add(new InputArgument(path));
            return container;
        }

        public static ArgumentContainer Input(this ArgumentContainer container, IEnumerable<VideoInfo> paths)
        {
            container.Add(new InputArgument(paths.ToArray()));
            return container;
        }

        public static ArgumentContainer Input(this ArgumentContainer container, params VideoInfo[] paths)
        {
            container.Add(new InputArgument(paths));
            return container;
        }

        public static ArgumentContainer Input(this ArgumentContainer container, FileInfo path)
        {
            container.Add(new InputArgument(path));
            return container;
        }

        public static ArgumentContainer Input(this ArgumentContainer container, IEnumerable<FileInfo> paths)
        {
            container.Add(new InputArgument(paths.ToArray()));
            return container;
        }

        public static ArgumentContainer Input(this ArgumentContainer container, params FileInfo[] paths)
        {
            container.Add(new InputArgument(paths));
            return container;
        }

        public static ArgumentContainer Input(this ArgumentContainer container, Uri uri)
        {
            container.Add(new InputArgument(uri));
            return container;
        }

        public static ArgumentContainer Input(this ArgumentContainer container, IEnumerable<Uri> uris)
        {
            container.Add(new InputArgument(uris.ToArray()));
            return container;
        }

        public static ArgumentContainer Input(this ArgumentContainer container, params Uri[] uris)
        {
            container.Add(new InputArgument(uris));
            return container;
        }

        public static ArgumentContainer Loop(this ArgumentContainer container, int times)
        {
            container.Add(new LoopArgument(times));
            return container;
        }

        public static ArgumentContainer Output(this ArgumentContainer container, string path)
        {
            container.Add(new OutputArgument(path));
            return container;
        }

        public static ArgumentContainer Output(this ArgumentContainer container, VideoInfo path)
        {
            container.Add(new OutputArgument(path));
            return container;
        }

        public static ArgumentContainer Output(this ArgumentContainer container, FileInfo path)
        {
            container.Add(new OutputArgument(path));
            return container;
        }

        public static ArgumentContainer Output(this ArgumentContainer container, Uri path)
        {
            container.Add(new OutputArgument(path));
            return container;
        }

        public static ArgumentContainer Override(this ArgumentContainer container)
        {
            container.Add(new OverrideArgument());
            return container;
        }

        public static ArgumentContainer RemoveMetadata(this ArgumentContainer container)
        {
            container.Add(new RemoveMetadataArgument());
            return container;
        }

        public static ArgumentContainer Scale(this ArgumentContainer container, Size value)
        {
            container.Add(new ScaleArgument(value));
            return container;
        }

        public static ArgumentContainer Scale(this ArgumentContainer container, VideoSize value)
        {
            container.Add(new ScaleArgument(value));
            return container;
        }

        public static ArgumentContainer Scale(this ArgumentContainer container, int width, int height)
        {
            container.Add(new ScaleArgument(width, height));
            return container;
        }

        public static ArgumentContainer Seek(this ArgumentContainer container, TimeSpan? value)
        {
            container.Add(new SeekArgument(value));
            return container;
        }

        public static ArgumentContainer Shortest(this ArgumentContainer container)
        {
            container.Add(new ShortestArgument(true));
            return container;
        }
        
        public static ArgumentContainer Size(this ArgumentContainer container, Size value)
        {
            container.Add(new SizeArgument(value));
            return container;
        }

        public static ArgumentContainer Size(this ArgumentContainer container, VideoSize value)
        {
            container.Add(new SizeArgument(value));
            return container;
        }

        public static ArgumentContainer Size(this ArgumentContainer container, int width, int height)
        {
            container.Add(new SizeArgument(width, height));
            return container;
        }

        public static ArgumentContainer Speed(this ArgumentContainer container, Speed value)
        {
            container.Add(new SpeedArgument(value));
            return container;
        }

        public static ArgumentContainer StartNumber(this ArgumentContainer container, int value)
        {
            container.Add(new StartNumberArgument(value));
            return container;
        }

        public static ArgumentContainer Threads(this ArgumentContainer container, int value)
        {
            container.Add(new ThreadsArgument(value));
            return container;
        }

        public static ArgumentContainer MultiThreaded(this ArgumentContainer container)
        {
            container.Add(new ThreadsArgument(true));
            return container;
        }

        public static ArgumentContainer Transpose(this ArgumentContainer container, int transpose)
        {
            container.Add(new TransposeArgument(transpose));
            return container;
        }

        public static ArgumentContainer VariableBitRate(this ArgumentContainer container, int vbr)
        {
            container.Add(new VariableBitRateArgument(vbr));
            return container;
        }

        public static ArgumentContainer VideoCodec(this ArgumentContainer container, VideoCodec codec)
        {
            container.Add(new VideoCodecArgument(codec));
            return container;
        }

        public static ArgumentContainer VideoCodec(this ArgumentContainer container, VideoCodec codec, int bitrate)
        {
            container.Add(new VideoCodecArgument(codec, bitrate));
            return container;
        }
    }
}
