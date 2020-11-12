using System;
using System.Drawing;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;

namespace FFMpegCore
{
    public class FFMpegArgumentOptions : FFMpegOptionsBase
    {
        internal FFMpegArgumentOptions() { }
        
        public FFMpegArgumentOptions WithAudioCodec(Codec audioCodec) => WithArgument(new AudioCodecArgument(audioCodec));
        public FFMpegArgumentOptions WithAudioCodec(string audioCodec) => WithArgument(new AudioCodecArgument(audioCodec));
        public FFMpegArgumentOptions WithAudioBitrate(AudioQuality audioQuality) => WithArgument(new AudioBitrateArgument(audioQuality));
        public FFMpegArgumentOptions WithAudioBitrate(int bitrate) => WithArgument(new AudioBitrateArgument(bitrate));
        public FFMpegArgumentOptions WithAudioSamplingRate(int samplingRate = 48000) => WithArgument(new AudioSamplingRateArgument(samplingRate));
        public FFMpegArgumentOptions WithVariableBitrate(int vbr) => WithArgument(new VariableBitRateArgument(vbr));
        
        public FFMpegArgumentOptions Resize(VideoSize videoSize) => WithArgument(new SizeArgument(videoSize));
        public FFMpegArgumentOptions Resize(int width, int height) => WithArgument(new SizeArgument(width, height));
        public FFMpegArgumentOptions Resize(Size? size) => WithArgument(new SizeArgument(size));
        
        public FFMpegArgumentOptions Scale(VideoSize videoSize) => WithArgument(new ScaleArgument(videoSize));
        public FFMpegArgumentOptions Scale(int width, int height) => WithArgument(new ScaleArgument(width, height));
        public FFMpegArgumentOptions Scale(Size size) => WithArgument(new ScaleArgument(size));
        
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
        public FFMpegArgumentOptions WithFramerate(double framerate) => WithArgument(new FrameRateArgument(framerate));
        public FFMpegArgumentOptions WithoutMetadata() => WithArgument(new RemoveMetadataArgument());
        public FFMpegArgumentOptions WithSpeedPreset(Speed speed) => WithArgument(new SpeedPresetArgument(speed));
        public FFMpegArgumentOptions WithStartNumber(int startNumber) => WithArgument(new StartNumberArgument(startNumber));
        public FFMpegArgumentOptions WithCustomArgument(string argument) => WithArgument(new CustomArgument(argument));
        
        public FFMpegArgumentOptions Seek(TimeSpan? seekTo) => WithArgument(new SeekArgument(seekTo));
        public FFMpegArgumentOptions Transpose(Transposition transposition) => WithArgument(new TransposeArgument(transposition));
        public FFMpegArgumentOptions Loop(int times) => WithArgument(new LoopArgument(times));
        public FFMpegArgumentOptions OverwriteExisting() => WithArgument(new OverwriteArgument());
        public FFMpegArgumentOptions SelectStream(int index) => WithArgument(new MapStreamArgument(index));

        public FFMpegArgumentOptions ForceFormat(ContainerFormat format) => WithArgument(new ForceFormatArgument(format));
        public FFMpegArgumentOptions ForceFormat(string format) => WithArgument(new ForceFormatArgument(format));
        public FFMpegArgumentOptions ForcePixelFormat(string pixelFormat) => WithArgument(new ForcePixelFormat(pixelFormat));
        public FFMpegArgumentOptions ForcePixelFormat(PixelFormat pixelFormat) => WithArgument(new ForcePixelFormat(pixelFormat));

        public FFMpegArgumentOptions DrawText(DrawTextOptions drawTextOptions) => WithArgument(new DrawTextArgument(drawTextOptions));

        public FFMpegArgumentOptions WithArgument(IArgument argument)
        {
            Arguments.Add(argument);
            return this;
        }
    }
}