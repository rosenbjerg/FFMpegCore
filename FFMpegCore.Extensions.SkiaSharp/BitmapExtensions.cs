using SkiaSharp;

namespace FFMpegCore.Extensions.SkiaSharp
{
    public static class BitmapExtensions
    {
        public static bool AddAudio(this SKBitmap poster, string audio, string output)
        {
            var destination = $"{Environment.TickCount}.png";
            using (var fileStream = File.OpenWrite(destination))
            {
                poster.Encode(fileStream, SKEncodedImageFormat.Png, default); // PNG does not respect the quality parameter
            }

            try
            {
                return FFMpeg.PosterWithAudio(destination, audio, output);
            }
            finally
            {
                if (File.Exists(destination))
                {
                    File.Delete(destination);
                }
            }
        }
    }
}
