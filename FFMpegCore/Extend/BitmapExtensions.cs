using System;
using System.Drawing;
using System.IO;
using FFMpegCore.FFMPEG;

namespace FFMpegCore.Extend
{
    public static class BitmapExtensions
    {
        public static VideoInfo AddAudio(this Bitmap poster, FileInfo audio, FileInfo output)
        {
            var destination = $"{Environment.TickCount}.png";

            poster.Save(destination);

            var tempFile = new FileInfo(destination);
            try
            {
                return new FFMpeg().PosterWithAudio(tempFile, audio, output);
            }
            finally
            {
                tempFile.Delete();
            }
        }
    }
}