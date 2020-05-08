using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore.FFMPEG.Enums;
using FFMpegCore.FFMPEG.Pipes;

namespace FFMpegCore.FFMPEG.Argument
{
    public class FFMpegArguments
    {
        private readonly IInputArgument _inputArgument;
        private IOutputArgument _outputArgument = null!;
        private readonly List<IArgument> _arguments;
        
        private FFMpegArguments(IInputArgument inputArgument)
        {
            _inputArgument = inputArgument;
            _arguments = new List<IArgument> { inputArgument };
        }

        public string Text => string.Join(" ", _arguments.Select(arg => arg.Text));
        
        public static FFMpegArguments FromInputFiles(params string[] files) => new FFMpegArguments(new InputArgument(true, files));
        public static FFMpegArguments FromInputFiles(bool verifyExists, params string[] files) => new FFMpegArguments(new InputArgument(verifyExists, files));
        public static FFMpegArguments FromInputFiles(params Uri[] uris) => new FFMpegArguments(new InputArgument(false, uris));
        public static FFMpegArguments FromInputFiles(bool verifyExists, params Uri[] uris) => new FFMpegArguments(new InputArgument(verifyExists, uris));
        public static FFMpegArguments FromInputFiles(params FileInfo[] files) => new FFMpegArguments(new InputArgument(false, files));
        public static FFMpegArguments FromInputFiles(bool verifyExists, params FileInfo[] files) => new FFMpegArguments(new InputArgument(verifyExists, files));
        public static FFMpegArguments FromConcatenation(params string[] files) => new FFMpegArguments(new ConcatArgument(files));
        public static FFMpegArguments FromPipe(IPipeDataWriter writer) => new FFMpegArguments(new InputPipeArgument(writer));

        
        public FFMpegArguments WithAudioCodec(AudioCodec audioCodec) => WithArgument(new AudioCodecArgument(audioCodec));
        public FFMpegArguments WithAudioBitrate(AudioQuality audioQuality) => WithArgument(new AudioBitrateArgument(audioQuality));
        public FFMpegArguments WithAudioBitrate(int bitrate) => WithArgument(new AudioBitrateArgument(bitrate));
        public FFMpegArguments WithAudioSamplingRate(int samplingRate = 48000) => WithArgument(new AudioSamplingRateArgument(samplingRate));
        public FFMpegArguments WithVariableBitrate(int vbr) => WithArgument(new VariableBitRateArgument(vbr));
        
        public FFMpegArguments Resize(VideoSize videoSize) => WithArgument(new SizeArgument(videoSize));
        public FFMpegArguments Resize(int width, int height) => WithArgument(new SizeArgument(width, height));
        public FFMpegArguments Resize(Size? size) => WithArgument(new SizeArgument(size));
        
        public FFMpegArguments Scale(VideoSize videoSize) => WithArgument(new ScaleArgument(videoSize));
        public FFMpegArguments Scale(int width, int height) => WithArgument(new ScaleArgument(width, height));
        public FFMpegArguments Scale(Size size) => WithArgument(new ScaleArgument(size));
        
        public FFMpegArguments WithBitStreamFilter(Channel channel, Filter filter) => WithArgument(new BitStreamFilterArgument(channel, filter));
        public FFMpegArguments WithConstantRateFactor(int crf) => WithArgument(new ConstantRateFactorArgument(crf));
        public FFMpegArguments CopyChannel(Channel channel = Channel.Both) => WithArgument(new CopyArgument(channel));
        public FFMpegArguments DisableChannel(Channel channel) => WithArgument(new DisableChannelArgument(channel));
        public FFMpegArguments WithDuration(TimeSpan? duration) => WithArgument(new DurationArgument(duration));
        public FFMpegArguments WithFastStart() => WithArgument(new FaststartArgument());
        public FFMpegArguments WithFrameOutputCount(int frames) => WithArgument(new FrameOutputCountArgument(frames));
        
        public FFMpegArguments UsingShortest(bool shortest = true) => WithArgument(new ShortestArgument(shortest));
        public FFMpegArguments UsingMultithreading(bool multithread) => WithArgument(new ThreadsArgument(multithread));
        public FFMpegArguments UsingThreads(int threads) => WithArgument(new ThreadsArgument(threads));
        
        public FFMpegArguments WithVideoCodec(VideoCodec videoCodec) => WithArgument(new VideoCodecArgument(videoCodec));
        public FFMpegArguments WithVideoCodec(string videoCodec) => WithArgument(new VideoCodecArgument(videoCodec));
        public FFMpegArguments WithVideoBitrate(int bitrate) => WithArgument(new VideoBitrateArgument(bitrate));
        public FFMpegArguments WithFramerate(double framerate) => WithArgument(new FrameRateArgument(framerate));
        public FFMpegArguments WithoutMetadata() => WithArgument(new RemoveMetadataArgument());
        public FFMpegArguments WithSpeedPreset(Speed speed) => WithArgument(new SpeedPresetArgument(speed));
        public FFMpegArguments WithStartNumber(int startNumber) => WithArgument(new StartNumberArgument(startNumber));
        public FFMpegArguments WithCpuSpeed(int cpuSpeed) => WithArgument(new CpuSpeedArgument(cpuSpeed));
        public FFMpegArguments WithCustomArgument(string argument) => WithArgument(new CustomArgument(argument));
        
        public FFMpegArguments Seek(TimeSpan? seekTo) => WithArgument(new SeekArgument(seekTo));
        public FFMpegArguments Transpose(TimeSpan? seekTo) => WithArgument(new SeekArgument(seekTo));
        public FFMpegArguments Loop(int times) => WithArgument(new LoopArgument(times));
        public FFMpegArguments OverwriteExisting() => WithArgument(new OverwriteArgument());
        public FFMpegArguments Quiet() => WithArgument(new QuietArgument());

        public FFMpegArguments ForceFormat(VideoCodec videoCodec) => WithArgument(new ForceFormatArgument(videoCodec));
        public FFMpegArguments ForceFormat(string videoCodec) => WithArgument(new ForceFormatArgument(videoCodec));
        public FFMpegArguments DrawText(DrawTextOptions drawTextOptions) => WithArgument(new DrawTextArgument(drawTextOptions));

        public FFMpegArgumentProcessor OutputToFile(string file, bool overwrite = false) => ToProcessor(new OutputArgument(file, overwrite));
        public FFMpegArgumentProcessor OutputToFile(Uri uri, bool overwrite = false) => ToProcessor(new OutputArgument(uri.AbsolutePath, overwrite));
        public FFMpegArgumentProcessor OutputToPipe(IPipeDataReader reader) => ToProcessor(new OutputPipeArgument(reader));

        public FFMpegArguments WithArgument(IArgument argument)
        {
            _arguments.Add(argument);
            return this;
        }
        private FFMpegArgumentProcessor ToProcessor(IOutputArgument argument)
        {
            _arguments.Add(argument);
            _outputArgument = argument;
            return new FFMpegArgumentProcessor(this);
        }

        internal void Pre()
        {
            _inputArgument.Pre();
            _outputArgument.Pre();
        }
        internal Task During(CancellationToken? cancellationToken = null)
        {
            return Task.WhenAll(_inputArgument.During(cancellationToken), _outputArgument.During(cancellationToken));
        }
        internal void Post()
        {
            _inputArgument.Post();
            _outputArgument.Post();
        }

        public TArgument? Find<TArgument>() where TArgument : class, IArgument
        {
            return _arguments.FirstOrDefault(arg => arg is TArgument) as TArgument;
        }
    }
}