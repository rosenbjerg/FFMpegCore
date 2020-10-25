using System.IO;
using System.Threading.Tasks;
using FFMpegCore.Test.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFMpegCore.Test
{
    [TestClass]
    public class FFProbeTests
    {
        [TestMethod]
        public void Probe_TooLongOutput()
        {
            Assert.ThrowsException<System.Text.Json.JsonException>(() => FFProbe.Analyse(VideoLibrary.LocalVideo.FullName, 5));
        }
        
        [TestMethod]
        public void Probe_Success()
        {
            var info = FFProbe.Analyse(VideoLibrary.LocalVideo.FullName);
            Assert.AreEqual(3, info.Duration.Seconds);
            Assert.AreEqual(".mp4", info.Extension);
            Assert.AreEqual(VideoLibrary.LocalVideo.FullName, info.Path);
            
            Assert.AreEqual("5.1", info.PrimaryAudioStream.ChannelLayout);
            Assert.AreEqual(6, info.PrimaryAudioStream.Channels);
            Assert.AreEqual("AAC (Advanced Audio Coding)", info.PrimaryAudioStream.CodecLongName);
            Assert.AreEqual("aac", info.PrimaryAudioStream.CodecName);
            Assert.AreEqual("LC", info.PrimaryAudioStream.Profile);
            Assert.AreEqual(377351, info.PrimaryAudioStream.BitRate);
            Assert.AreEqual(48000, info.PrimaryAudioStream.SampleRateHz);
            
            Assert.AreEqual(1471810, info.PrimaryVideoStream.BitRate);
            Assert.AreEqual(16, info.PrimaryVideoStream.DisplayAspectRatio.Width);
            Assert.AreEqual(9, info.PrimaryVideoStream.DisplayAspectRatio.Height);
            Assert.AreEqual("yuv420p", info.PrimaryVideoStream.PixelFormat);
            Assert.AreEqual(1280, info.PrimaryVideoStream.Width);
            Assert.AreEqual(720, info.PrimaryVideoStream.Height);
            Assert.AreEqual(25, info.PrimaryVideoStream.AvgFrameRate);
            Assert.AreEqual(25, info.PrimaryVideoStream.FrameRate);
            Assert.AreEqual("H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10", info.PrimaryVideoStream.CodecLongName);
            Assert.AreEqual("h264", info.PrimaryVideoStream.CodecName);
            Assert.AreEqual(8, info.PrimaryVideoStream.BitsPerRawSample);
            Assert.AreEqual("Main", info.PrimaryVideoStream.Profile);
        }
        
        [TestMethod]
        public async Task Probe_Async_Success()
        {
            var info = await FFProbe.AnalyseAsync(VideoLibrary.LocalVideo.FullName);
            Assert.AreEqual(3, info.Duration.Seconds);
        }

        [TestMethod, Timeout(10000)]
        public void Probe_Success_FromStream()
        {
            using var stream = File.OpenRead(VideoLibrary.LocalVideoWebm.FullName);
            var info = FFProbe.Analyse(stream);
            Assert.AreEqual(3, info.Duration.Seconds);
        }

        [TestMethod]
        public async Task Probe_Success_FromStream_Async()
        {
            await using var stream = File.OpenRead(VideoLibrary.LocalVideoWebm.FullName);
            var info = await FFProbe.AnalyseAsync(stream);
            Assert.AreEqual(3, info.Duration.Seconds);
        }
    }
}