using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
using FFMpegCore.Pipes;
using FFMpegCore.Test.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FFMpegCore.Test
{
    [TestClass]
    public class AudioTest 
    {
        [TestMethod]
        public void Audio_Remove()
        {
            using var outputFile = new TemporaryFile("out.mp4");
            
            FFMpeg.Mute(TestResources.Mp4Video, outputFile);
            var analysis = FFProbe.Analyse(outputFile);
            
            Assert.IsTrue(analysis.VideoStreams.Any());
            Assert.IsTrue(!analysis.AudioStreams.Any());
        }

        [TestMethod]
        public void Audio_Save()
        {
            using var outputFile = new TemporaryFile("out.mp3");
            
            FFMpeg.ExtractAudio(TestResources.Mp4Video, outputFile);
            var analysis = FFProbe.Analyse(outputFile);
            
            Assert.IsTrue(!analysis.VideoStreams.Any());
            Assert.IsTrue(analysis.AudioStreams.Any());
        }
        [TestMethod]
        public async Task Audio_FromRaw()
        {
            await using var file = File.Open(TestResources.RawAudio, FileMode.Open);
            var memoryStream = new MemoryStream();
            await FFMpegArguments
                .FromPipeInput(new StreamPipeSource(file), options => options.ForceFormat("s16le"))
                .OutputToPipe(new StreamPipeSink(memoryStream), options => options.ForceFormat("mp3"))
                .ProcessAsynchronously();
        }
        
        [TestMethod]
        public void Audio_Add()
        {
            using var outputFile = new TemporaryFile("out.mp4");
            
            var success = FFMpeg.ReplaceAudio(TestResources.Mp4WithoutAudio, TestResources.Mp3Audio, outputFile);
            var videoAnalysis = FFProbe.Analyse(TestResources.Mp4WithoutAudio);
            var audioAnalysis = FFProbe.Analyse(TestResources.Mp3Audio);
            var outputAnalysis = FFProbe.Analyse(outputFile);
            
            Assert.IsTrue(success);
            Assert.AreEqual(Math.Max(videoAnalysis.Duration.TotalSeconds, audioAnalysis.Duration.TotalSeconds), outputAnalysis.Duration.TotalSeconds, 0.15);
            Assert.IsTrue(File.Exists(outputFile));
        }

        [TestMethod]
        public void Image_AddAudio()
        {
            using var outputFile = new TemporaryFile("out.mp4");
            FFMpeg.PosterWithAudio(TestResources.PngImage, TestResources.Mp3Audio, outputFile);
            var analysis = FFProbe.Analyse(TestResources.Mp3Audio);
            Assert.IsTrue(analysis.Duration.TotalSeconds > 0);
            Assert.IsTrue(File.Exists(outputFile));
        }

        [TestMethod, Timeout(10000)]
        public void Audio_ToAAC_Args_Pipe()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

            var samples = new List<IAudioSample>
            {
                new PcmAudioSampleWrapper(new byte[] { 0, 0 }),
                new PcmAudioSampleWrapper(new byte[] { 0, 0 }),
            };

            var audioSamplesSource = new RawAudioPipeSource(samples)
            {
                Channels = 2,
                Format = "s8",
                SampleRate = 8000,
            };

            var success = FFMpegArguments
                .FromPipeInput(audioSamplesSource)
                .OutputToFile(outputFile, false, opt => opt
                    .WithAudioCodec(AudioCodec.Aac))
                .ProcessSynchronously();
            Assert.IsTrue(success);
        }

        [TestMethod, Timeout(10000)]
        public void Audio_ToLibVorbis_Args_Pipe()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

            var samples = new List<IAudioSample>
            {
                new PcmAudioSampleWrapper(new byte[] { 0, 0 }),
                new PcmAudioSampleWrapper(new byte[] { 0, 0 }),
            };

            var audioSamplesSource = new RawAudioPipeSource(samples)
            {
                Channels = 2,
                Format = "s8",
                SampleRate = 8000,
            };

            var success = FFMpegArguments
                .FromPipeInput(audioSamplesSource)
                .OutputToFile(outputFile, false, opt => opt
                    .WithAudioCodec(AudioCodec.LibVorbis))
                .ProcessSynchronously();
            Assert.IsTrue(success);
        }

        [TestMethod, Timeout(10000)]
        public async Task Audio_ToAAC_Args_Pipe_Async()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

            var samples = new List<IAudioSample>
            {
                new PcmAudioSampleWrapper(new byte[] { 0, 0 }),
                new PcmAudioSampleWrapper(new byte[] { 0, 0 }),
            };

            var audioSamplesSource = new RawAudioPipeSource(samples)
            {
                Channels = 2,
                Format = "s8",
                SampleRate = 8000,
            };

            var success = await FFMpegArguments
                .FromPipeInput(audioSamplesSource)
                .OutputToFile(outputFile, false, opt => opt
                    .WithAudioCodec(AudioCodec.Aac))
                .ProcessAsynchronously();
            Assert.IsTrue(success);
        }

        [TestMethod, Timeout(10000)]
        public void Audio_ToAAC_Args_Pipe_ValidDefaultConfiguration()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

            var samples = new List<IAudioSample>
            {
                new PcmAudioSampleWrapper(new byte[] { 0, 0 }),
                new PcmAudioSampleWrapper(new byte[] { 0, 0 }),
            };

            var audioSamplesSource = new RawAudioPipeSource(samples);

            var success = FFMpegArguments
                .FromPipeInput(audioSamplesSource)
                .OutputToFile(outputFile, false, opt => opt
                    .WithAudioCodec(AudioCodec.Aac))
                .ProcessSynchronously();
            Assert.IsTrue(success);
        }

        [TestMethod, Timeout(10000)]
        public void Audio_ToAAC_Args_Pipe_InvalidChannels()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

            var audioSamplesSource = new RawAudioPipeSource(new List<IAudioSample>())
            {
                Channels = 0,
            };

            var ex = Assert.ThrowsException<FFMpegException>(() => FFMpegArguments
                .FromPipeInput(audioSamplesSource)
                .OutputToFile(outputFile, false, opt => opt
                    .WithAudioCodec(AudioCodec.Aac))
                .ProcessSynchronously());
        }

        [TestMethod, Timeout(10000)]
        public void Audio_ToAAC_Args_Pipe_InvalidFormat()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

            var audioSamplesSource = new RawAudioPipeSource(new List<IAudioSample>())
            {
                Format = "s8le",
            };

            var ex = Assert.ThrowsException<FFMpegException>(() => FFMpegArguments
                .FromPipeInput(audioSamplesSource)
                .OutputToFile(outputFile, false, opt => opt
                    .WithAudioCodec(AudioCodec.Aac))
                .ProcessSynchronously());
        }

        [TestMethod, Timeout(10000)]
        public void Audio_ToAAC_Args_Pipe_InvalidSampleRate()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

            var audioSamplesSource = new RawAudioPipeSource(new List<IAudioSample>())
            {
                SampleRate = 0,
            };

            var ex = Assert.ThrowsException<FFMpegException>(() => FFMpegArguments
                .FromPipeInput(audioSamplesSource)
                .OutputToFile(outputFile, false, opt => opt
                    .WithAudioCodec(AudioCodec.Aac))
                .ProcessSynchronously());
        }
    }
}