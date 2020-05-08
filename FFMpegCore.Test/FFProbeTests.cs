using System.IO;
using System.Threading.Tasks;
using FFMpegCore.FFMPEG;
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
            Assert.AreEqual(13, info.Duration.Seconds);
        }

        [TestMethod]
        public void Probe_Success_FromStream()
        {
            using var stream = File.OpenRead(VideoLibrary.LocalVideo.FullName);
            var info = FFProbe.Analyse(stream);
            Assert.AreEqual(13, info.Duration.Seconds);
        }

        [TestMethod]
        public async Task Probe_Success_FromStream_Async()
        {
            await using var stream = File.OpenRead(VideoLibrary.LocalVideo.FullName);
            var info = await FFProbe.AnalyseAsync(stream);
            Assert.AreEqual(13, info.Duration.Seconds);
        }
    }
}