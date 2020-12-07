using System;
using FFMpegCore.Enums;
using FFMpegCore.Test.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FFMpegCore.Pipes;

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
            await using var file = File.Open(VideoLibrary.LocalAudioRaw.FullName, FileMode.Open);
            var memoryStream = new MemoryStream();
            await FFMpegArguments
                .FromPipeInput(new StreamPipeSource(file), options => options.ForceFormat("s16le"))
                .OutputToPipe(new StreamPipeSink(memoryStream), options =>
                {
                    options.WithAudioSamplingRate(48000);
                    options.WithAudioCodec("libopus");
                    options.WithCustomArgument("-ac 2");
                    options.ForceFormat("opus");
                })
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
    }
}