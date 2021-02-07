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
            Assert.ThrowsException<System.Text.Json.JsonException>(() => FFProbe.Analyse(TestResources.Mp4Video, 5));
        }
        

        [TestMethod]
        public async Task Audio_FromStream_Duration()
        {
            var fileAnalysis = await FFProbe.AnalyseAsync(TestResources.WebmVideo);
            await using var inputStream = File.OpenRead(TestResources.WebmVideo);
            var streamAnalysis = await FFProbe.AnalyseAsync(inputStream);
            Assert.IsTrue(fileAnalysis.Duration == streamAnalysis.Duration);
        }

        [TestMethod]
        public void MediaAnalysis_ParseDuration()
        {
            var durationHHMMSS = new FFProbeStream { Duration = "05:12:59.177" };
            var longDuration = new FFProbeStream { Duration = "149:07:50.911750" };
            var shortDuration = new FFProbeStream { Duration = "00:00:00.83" };

            var testdurationHHMMSS = MediaAnalysis.ParseDuration(durationHHMMSS);
            var testlongDuration = MediaAnalysis.ParseDuration(longDuration);
            var testshortDuration = MediaAnalysis.ParseDuration(shortDuration);

            Assert.IsTrue(testdurationHHMMSS.Days == 0 && testdurationHHMMSS.Hours == 5 && testdurationHHMMSS.Minutes == 12 && testdurationHHMMSS.Seconds == 59 && testdurationHHMMSS.Milliseconds == 177);
            Assert.IsTrue(testlongDuration.Days == 6 && testlongDuration.Hours == 5 && testlongDuration.Minutes == 7 && testlongDuration.Seconds == 50 && testlongDuration.Milliseconds == 911);
            Assert.IsTrue(testdurationHHMMSS.Days == 0 && testshortDuration.Hours == 0 && testshortDuration.Minutes == 0 && testshortDuration.Seconds == 0 && testshortDuration.Milliseconds == 830);
        }

        [TestMethod]
        public void Probe_Success()
        {
            var info = FFProbe.Analyse(TestResources.Mp4Video);
            Assert.AreEqual(3, info.Duration.Seconds);
            Assert.AreEqual(".mp4", info.Extension);
            Assert.AreEqual(TestResources.Mp4Video, info.Path);
            
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
        
        [TestMethod, Timeout(10000)]
        public async Task Probe_Async_Success()
        {
            var info = await FFProbe.AnalyseAsync(TestResources.Mp4Video);
            Assert.AreEqual(3, info.Duration.Seconds);
        }

        [TestMethod, Timeout(10000)]
        public void Probe_Success_FromStream()
        {
            using var stream = File.OpenRead(TestResources.WebmVideo);
            var info = FFProbe.Analyse(stream);
            Assert.AreEqual(3, info.Duration.Seconds);
        }

        [TestMethod, Timeout(10000)]
        public async Task Probe_Success_FromStream_Async()
        {
            await using var stream = File.OpenRead(TestResources.WebmVideo);
            var info = await FFProbe.AnalyseAsync(stream);
            Assert.AreEqual(3, info.Duration.Seconds);
        }
    }
}