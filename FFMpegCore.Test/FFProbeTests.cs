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
    }
}