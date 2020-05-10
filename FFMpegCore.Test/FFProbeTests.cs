using System.IO;
using FFMpegCore.Enums;
using FFMpegCore.FFMPEG;
using FFMpegCore.FFMPEG.Argument;
using FFMpegCore.Test.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace FFMpegCore.Test
{
    [TestClass]
    public class FFProbeTests
    {
        [TestMethod]
        public void Probe_TooLongOutput()
        {
            var output = new FFProbe(5);

            Assert.ThrowsException<JsonSerializationException>(() =>
            {
                output.ParseVideoInfo(VideoLibrary.LocalVideo.FullName);
            });
        }
        
        [TestMethod]
        public void Probe_Success()
        {
            var output = new FFProbe();

            var info = output.ParseVideoInfo(VideoLibrary.LocalVideo.FullName);
            
            Assert.AreEqual(13, info.Duration.Seconds);
        }

        [TestMethod]
        public void Probe_Success_FromStream()
        {
            var output = new FFProbe();

            using (var stream = File.OpenRead(VideoLibrary.LocalVideoWebm.FullName))
            {
                var info = output.ParseVideoInfo(stream);
                Assert.AreEqual(10, info.Duration.Seconds);
            }
        }

        [TestMethod]
        public void Probe_Success_FromStream_Async()
        {
            var output = new FFProbe();

            using (var stream = File.OpenRead(VideoLibrary.LocalVideoWebm.FullName))
            {
                var info = output.ParseVideoInfoAsync(stream).WaitForResult();
                
                Assert.AreEqual(10, info.Duration.Seconds);
            }
        }
    }
}