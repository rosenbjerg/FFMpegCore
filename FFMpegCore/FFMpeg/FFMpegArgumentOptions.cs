using System.Drawing;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;

namespace FFMpegCore
{
    public class FFMpegArgumentOptions : FFMpegArgumentsBase
    {
        internal FFMpegArgumentOptions() { }

        public FFMpegArgumentOptions WithAudioCodec(Codec audioCodec) => WithArgument(new AudioCodecArgument(audioCodec));
        public FFMpegArgumentOptions WithAudioCodec(string audioCodec) => WithArgument(new AudioCodecArgument(audioCodec));
        public FFMpegArgumentOptions WithAudioBitrate(AudioQuality audioQuality) => WithArgument(new AudioBitrateArgument(audioQuality));
        public FFMpegArgumentOptions WithAudioBitrate(int bitrate) => WithArgument(new AudioBitrateArgument(bitrate));
        public FFMpegArgumentOptions WithAudioSamplingRate(int samplingRate = 48000) => WithArgument(new AudioSamplingRateArgument(samplingRate));
        public FFMpegArgumentOptions WithVariableBitrate(int vbr) => WithArgument(new VariableBitRateArgument(vbr));
        public FFMpegArgumentOptions Resize(int width, int height) => WithArgument(new SizeArgument(width, height));
        public FFMpegArgumentOptions Resize(Size? size) => WithArgument(new SizeArgument(size));

        public FFMpegArgumentOptions WithBitStreamFilter(Channel channel, Filter filter) => WithArgument(new BitStreamFilterArgument(channel, filter));
        public FFMpegArgumentOptions WithConstantRateFactor(int crf) => WithArgument(new ConstantRateFactorArgument(crf));
        public FFMpegArgumentOptions CopyChannel(Channel channel = Channel.Both) => WithArgument(new CopyArgument(channel));
        public FFMpegArgumentOptions DisableChannel(Channel channel) => WithArgument(new DisableChannelArgument(channel));
        public FFMpegArgumentOptions WithDuration(TimeSpan? duration) => WithArgument(new DurationArgument(duration));
        public FFMpegArgumentOptions WithFastStart() => WithArgument(new FaststartArgument());
        public FFMpegArgumentOptions WithFrameOutputCount(int frames) => WithArgument(new FrameOutputCountArgument(frames));
        public FFMpegArgumentOptions WithHardwareAcceleration(HardwareAccelerationDevice hardwareAccelerationDevice = HardwareAccelerationDevice.Auto) => WithArgument(new HardwareAccelerationArgument(hardwareAccelerationDevice));

        public FFMpegArgumentOptions UsingShortest(bool shortest = true) => WithArgument(new ShortestArgument(shortest));
        public FFMpegArgumentOptions UsingMultithreading(bool multithread) => WithArgument(new ThreadsArgument(multithread));
        public FFMpegArgumentOptions UsingThreads(int threads) => WithArgument(new ThreadsArgument(threads));

        public FFMpegArgumentOptions WithVideoCodec(Codec videoCodec) => WithArgument(new VideoCodecArgument(videoCodec));
        public FFMpegArgumentOptions WithVideoCodec(string videoCodec) => WithArgument(new VideoCodecArgument(videoCodec));
        public FFMpegArgumentOptions WithVideoBitrate(int bitrate) => WithArgument(new VideoBitrateArgument(bitrate));
        public FFMpegArgumentOptions WithVideoFilters(Action<VideoFilterOptions> videoFilterOptions)
        {
            var videoFilterOptionsObj = new VideoFilterOptions();
            videoFilterOptions(videoFilterOptionsObj);
            return WithArgument(new VideoFiltersArgument(videoFilterOptionsObj));
        }

        public FFMpegArgumentOptions WithAudioFilters(Action<AudioFilterOptions> audioFilterOptions)
        {
            var audioFilterOptionsObj = new AudioFilterOptions();
            audioFilterOptions(audioFilterOptionsObj);
            return WithArgument(new AudioFiltersArgument(audioFilterOptionsObj));
        }

        public FFMpegArgumentOptions WithFramerate(double framerate) => WithArgument(new FrameRateArgument(framerate));
        public FFMpegArgumentOptions WithoutMetadata() => WithArgument(new RemoveMetadataArgument());
        public FFMpegArgumentOptions WithSpeedPreset(Speed speed) => WithArgument(new SpeedPresetArgument(speed));
        public FFMpegArgumentOptions WithStartNumber(int startNumber) => WithArgument(new StartNumberArgument(startNumber));
        public FFMpegArgumentOptions WithCustomArgument(string argument) => WithArgument(new CustomArgument(argument));

        public FFMpegArgumentOptions Seek(TimeSpan? seekTo) => WithArgument(new SeekArgument(seekTo));
        public FFMpegArgumentOptions EndSeek(TimeSpan? seekTo) => WithArgument(new EndSeekArgument(seekTo));
        public FFMpegArgumentOptions Loop(int times) => WithArgument(new LoopArgument(times));
        public FFMpegArgumentOptions OverwriteExisting() => WithArgument(new OverwriteArgument());
        public FFMpegArgumentOptions SelectStream(int streamIndex, int inputFileIndex = 0,
            Channel channel = Channel.All) => WithArgument(new MapStreamArgument(streamIndex, inputFileIndex, channel));
        public FFMpegArgumentOptions SelectStreams(IEnumerable<int> streamIndices, int inputFileIndex = 0,
            Channel channel = Channel.All) => streamIndices.Aggregate(this,
            (options, streamIndex) => options.SelectStream(streamIndex, inputFileIndex, channel));
        public FFMpegArgumentOptions DeselectStream(int streamIndex, int inputFileIndex = 0,
            Channel channel = Channel.All) => WithArgument(new MapStreamArgument(streamIndex, inputFileIndex, channel, true));
        public FFMpegArgumentOptions DeselectStreams(IEnumerable<int> streamIndices, int inputFileIndex = 0,
            Channel channel = Channel.All) => streamIndices.Aggregate(this,
            (options, streamIndex) => options.DeselectStream(streamIndex, inputFileIndex, channel));

        public FFMpegArgumentOptions ForceFormat(ContainerFormat format) => WithArgument(new ForceFormatArgument(format));
        public FFMpegArgumentOptions ForceFormat(string format) => WithArgument(new ForceFormatArgument(format));
        public FFMpegArgumentOptions ForcePixelFormat(string pixelFormat) => WithArgument(new ForcePixelFormat(pixelFormat));
        public FFMpegArgumentOptions ForcePixelFormat(PixelFormat pixelFormat) => WithArgument(new ForcePixelFormat(pixelFormat));

        public FFMpegArgumentOptions WithAudibleEncryptionKeys(string key, string iv) => WithArgument(new AudibleEncryptionKeyArgument(key, iv));
        public FFMpegArgumentOptions WithAudibleActivationBytes(string activationBytes) => WithArgument(new AudibleEncryptionKeyArgument(activationBytes));
        public FFMpegArgumentOptions WithTagVersion(int id3v2Version = 3) => WithArgument(new ID3V2VersionArgument(id3v2Version));
        public FFMpegArgumentOptions WithGifPaletteArgument(int streamIndex, Size? size, int fps = 12) => WithArgument(new GifPaletteArgument(streamIndex, fps, size));

        public FFMpegArgumentOptions WithArgument(IArgument argument)
        {
            Arguments.Add(argument);
            return this;
        }
    }
}
