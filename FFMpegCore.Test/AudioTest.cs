using System;
using FFMpegCore.Enums;
using FFMpegCore.Test.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace FFMpegCore.Test
{
    [TestClass]
    public class AudioTest : BaseTest
    {
        [TestMethod]
        public void Audio_Remove()
        {
            var output = Input.OutputLocation(VideoType.Mp4);

            try
            {
                FFMpeg.Mute(Input.FullName, output);
                Assert.IsTrue(File.Exists(output));
            }
            finally
            {
                if (File.Exists(output)) File.Delete(output);
            }
        }

        [TestMethod]
        public void Audio_Save()
        {
            var output = Input.OutputLocation(AudioType.Mp3);

            try
            {
                FFMpeg.ExtractAudio(Input.FullName, output);
                Assert.IsTrue(File.Exists(output));
            }
            finally
            {
                if (File.Exists(output)) File.Delete(output);
            }
        }

        [TestMethod]
        public void Audio_Add()
        {
            var output = Input.OutputLocation(VideoType.Mp4);
            try
            {
                var success = FFMpeg.ReplaceAudio(VideoLibrary.LocalVideoNoAudio.FullName, VideoLibrary.LocalAudio.FullName, output);
                Assert.IsTrue(success);
                var audioAnalysis = FFProbe.Analyse(VideoLibrary.LocalVideoNoAudio.FullName);
                var videoAnalysis = FFProbe.Analyse(VideoLibrary.LocalAudio.FullName);
                var outputAnalysis = FFProbe.Analyse(output);
                Assert.AreEqual(Math.Max(videoAnalysis.Duration.TotalSeconds, audioAnalysis.Duration.TotalSeconds), outputAnalysis.Duration.TotalSeconds, 0.15);
                Assert.IsTrue(File.Exists(output));
            }
            finally
            {
                if (File.Exists(output)) File.Delete(output);
            }
        }

        [TestMethod]
        public void Image_AddAudio()
        {
            var output = Input.OutputLocation(VideoType.Mp4);

            try
            {
                FFMpeg.PosterWithAudio(VideoLibrary.LocalCover.FullName, VideoLibrary.LocalAudio.FullName, output);
                var analysis = FFProbe.Analyse(VideoLibrary.LocalAudio.FullName);
                Assert.IsTrue(analysis.Duration.TotalSeconds > 0);
                Assert.IsTrue(File.Exists(output));
            }
            finally
            {
                if (File.Exists(output)) File.Delete(output);
            }
        }
    }
}