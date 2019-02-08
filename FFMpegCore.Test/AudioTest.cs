using FFMpegCore.Enums;
using FFMpegCore.Tests.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace FFMpegCore.Tests
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
                Encoder.Mute(VideoInfo.FromFileInfo(Input), output);

                Assert.IsTrue(File.Exists(output.FullName));
            }
            finally
            {
                if (File.Exists(output.FullName))
                    output.Delete();
            }
        }

        [TestMethod]
        public void Audio_Save()
        {
            var output = Input.OutputLocation(AudioType.Mp3);

            try
            {
                Encoder.ExtractAudio(VideoInfo.FromFileInfo(Input), output);

                Assert.IsTrue(File.Exists(output.FullName));
            }
            finally
            {
                if (File.Exists(output.FullName))
                    output.Delete();
            }
        }

        [TestMethod]
        public void Audio_Add()
        {
            var output = Input.OutputLocation(VideoType.Mp4);
            try
            {
                var input = VideoInfo.FromFileInfo(VideoLibrary.LocalVideoNoAudio);
                Encoder.ReplaceAudio(input, VideoLibrary.LocalAudio, output);

                Assert.AreEqual(input.Duration, VideoInfo.FromFileInfo(output).Duration);
                Assert.IsTrue(File.Exists(output.FullName));
            }
            finally
            {
                if (File.Exists(output.FullName))
                    output.Delete();
            }
        }

        [TestMethod]
        public void Image_AddAudio()
        {
            var output = Input.OutputLocation(VideoType.Mp4);

            try
            {
                var result = Encoder.PosterWithAudio(new FileInfo(VideoLibrary.LocalCover.FullName), VideoLibrary.LocalAudio, output);
                Assert.IsTrue(result.Duration.TotalSeconds > 0);
                Assert.IsTrue(result.Exists);
            }
            finally
            {
                if (File.Exists(output.FullName))
                    output.Delete();
            }
        }
    }
}