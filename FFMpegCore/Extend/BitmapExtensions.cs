using System;
using System.Drawing;
using System.IO;

namespace FFMpegCore.Extend
{
    public static class BitmapExtensions
    {
        public static bool AddAudio(this Image poster, string audio, string output)
        {
            var destination = $"{Environment.TickCount}.png";
            poster.Save(destination);
            try
            {
                return FFMpeg.PosterWithAudio(destination, audio, output);
            }
            finally
            {
                if (File.Exists(destination)) File.Delete(destination);
            }
        }
    }
}