using FFMpegCore.FFMPEG;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFMpegCore.Test
{
    [TestClass]
    public class FFMpegTest
    {
        [TestMethod]
        public void CTOR_Default()
        {
            var encoder = new FFMpeg();
            var probe = new FFProbe();

            Assert.IsNotNull(encoder);
            Assert.IsNotNull(probe);
        }
        
        [TestMethod]
        public void TestWriteImageSequence()
        {
            var encoder = new FFMpeg();

            var bitmapPaths = new List<string> {@".\temp\test-image-1.png", @".\temp\test-image-2.png"};


            foreach (var path in bitmapPaths)
            {
                const int sideLength = 16;
                var bitmap = new Bitmap(sideLength, sideLength);
                var graphics = Graphics.FromImage(bitmap);
                graphics.FillRectangle(Brushes.White, 0, 0, sideLength, sideLength);
                graphics.DrawLine(Pens.Black, 0, 0, sideLength, sideLength);
                bitmap.Save(path, ImageFormat.Png);
            }

            // This tests fails if this throws an exception.
            encoder.JoinImageSequence(new FileInfo(@".\temp\test-image-sequence.mp4"),
                images: bitmapPaths.Select(ImageInfo.FromPath).ToArray());
        }
    }
}
