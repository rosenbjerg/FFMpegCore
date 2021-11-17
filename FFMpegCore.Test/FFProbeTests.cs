using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public void FrameAnalysis_Sync()
        {
            var frameAnalysis = FFProbe.GetFrames(TestResources.WebmVideo);
            
            Assert.AreEqual(90, frameAnalysis.Frames.Count);
            Assert.IsTrue(frameAnalysis.Frames.All(f => f.PixelFormat == "yuv420p"));
            Assert.IsTrue(frameAnalysis.Frames.All(f => f.Height == 360));
            Assert.IsTrue(frameAnalysis.Frames.All(f => f.Width == 640));
            Assert.IsTrue(frameAnalysis.Frames.All(f => f.MediaType == "video"));
        }

        [TestMethod]
        public async Task FrameAnalysis_Async()
        {
            var frameAnalysis = await FFProbe.GetFramesAsync(TestResources.WebmVideo);
            
            Assert.AreEqual(90, frameAnalysis.Frames.Count);
            Assert.IsTrue(frameAnalysis.Frames.All(f => f.PixelFormat == "yuv420p"));
            Assert.IsTrue(frameAnalysis.Frames.All(f => f.Height == 360));
            Assert.IsTrue(frameAnalysis.Frames.All(f => f.Width == 640));
            Assert.IsTrue(frameAnalysis.Frames.All(f => f.MediaType == "video"));
        }

        [TestMethod]
        public async Task PacketAnalysis_Async()
        {
            var packetAnalysis = await FFProbe.GetPacketsAsync(TestResources.WebmVideo);
            var packets = packetAnalysis.Packets;
            Assert.AreEqual(96, packets.Count);
            Assert.IsTrue(packets.All(f => f.CodecType == "video"));
            Assert.AreEqual("K_", packets[0].Flags);
            Assert.AreEqual(1362, packets.Last().Size);
        }

        
        [TestMethod]
        public void PacketAnalysis_Sync()
        {
            var packets = FFProbe.GetPackets(TestResources.WebmVideo).Packets;
            
            Assert.AreEqual(96, packets.Count);
            Assert.IsTrue(packets.All(f => f.CodecType == "video"));
            Assert.AreEqual("K_", packets[0].Flags);
            Assert.AreEqual(1362, packets.Last().Size);
        }

        [TestMethod]
        public void PacketAnalysisAudioVideo_Sync()
        {
            var packets = FFProbe.GetPackets(TestResources.Mp4Video).Packets;

            Assert.AreEqual(216, packets.Count);
            var actual = packets.Select(f => f.CodecType).Distinct().ToList();
            var expected = new List<string> {"audio", "video"};
            CollectionAssert.AreEquivalent(expected, actual);
            Assert.IsTrue(packets.Where(t=>t.CodecType == "audio").All(f => f.Flags == "K_"));
            Assert.AreEqual(75, packets.Count(t => t.CodecType == "video"));
            Assert.AreEqual(141, packets.Count(t => t.CodecType == "audio"));
        }

        [DataTestMethod]
        [DataRow("0:00:03.008000", 0, 0, 0, 3, 8)]
        [DataRow("05:12:59.177", 0, 5, 12, 59, 177)]
        [DataRow("149:07:50.911750", 6, 5, 7, 50, 911)]
        [DataRow("00:00:00.83", 0, 0, 0, 0, 830)]
        public void MediaAnalysis_ParseDuration(string duration, int expectedDays, int expectedHours, int expectedMinutes, int expectedSeconds, int expectedMilliseconds)
        {
            var ffprobeStream = new FFProbeStream { Duration = duration };

            var parsedDuration = MediaAnalysisUtils.ParseDuration(ffprobeStream);

            Assert.AreEqual(expectedDays, parsedDuration.Days);
            Assert.AreEqual(expectedHours, parsedDuration.Hours);
            Assert.AreEqual(expectedMinutes, parsedDuration.Minutes);
            Assert.AreEqual(expectedSeconds, parsedDuration.Seconds);
            Assert.AreEqual(expectedMilliseconds, parsedDuration.Milliseconds);
        }

        [TestMethod]
        public async Task Uri_Duration()
        {
            var fileAnalysis = await FFProbe.AnalyseAsync(new Uri("https://github.com/rosenbjerg/FFMpegCore/raw/master/FFMpegCore.Test/Resources/input_3sec.webm"));
            Assert.IsNotNull(fileAnalysis);
        }
        
        [TestMethod]
        public void Probe_Success()
        {
            var info = FFProbe.Analyse(TestResources.Mp4Video);
            Assert.AreEqual(3, info.Duration.Seconds);
            
            Assert.AreEqual("5.1", info.PrimaryAudioStream!.ChannelLayout);
            Assert.AreEqual(6, info.PrimaryAudioStream.Channels);
            Assert.AreEqual("AAC (Advanced Audio Coding)", info.PrimaryAudioStream.CodecLongName);
            Assert.AreEqual("aac", info.PrimaryAudioStream.CodecName);
            Assert.AreEqual("LC", info.PrimaryAudioStream.Profile);
            Assert.AreEqual(377351, info.PrimaryAudioStream.BitRate);
            Assert.AreEqual(48000, info.PrimaryAudioStream.SampleRateHz);
            Assert.AreEqual("mp4a", info.PrimaryAudioStream.CodecTagString);
            Assert.AreEqual("0x6134706d", info.PrimaryAudioStream.CodecTag);
            
            Assert.AreEqual(1471810, info.PrimaryVideoStream!.BitRate);
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
            Assert.AreEqual("avc1", info.PrimaryVideoStream.CodecTagString);
            Assert.AreEqual("0x31637661", info.PrimaryVideoStream.CodecTag);
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

        [TestMethod, Timeout(10000)]
        public async Task Probe_Success_Subtitle_Async()
        {
            var info = await FFProbe.AnalyseAsync(TestResources.SrtSubtitle);
            Assert.IsNotNull(info.PrimarySubtitleStream);
            Assert.AreEqual(1, info.SubtitleStreams.Count);
            Assert.AreEqual(0, info.AudioStreams.Count);
            Assert.AreEqual(0, info.VideoStreams.Count);
        }

        [TestMethod, Timeout(10000)]
        public async Task Probe_Success_Disposition_Async()
        {
            var info = await FFProbe.AnalyseAsync(TestResources.Mp4Video);
            Assert.IsNotNull(info.PrimaryAudioStream);
            Assert.IsNotNull(info.PrimaryAudioStream.Disposition);
            Assert.AreEqual(true, info.PrimaryAudioStream.Disposition["default"]);
            Assert.AreEqual(false, info.PrimaryAudioStream.Disposition["forced"]);
        }
    }
}