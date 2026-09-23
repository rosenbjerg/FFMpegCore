using SkiaSharp;

namespace FFMpegCore.Extensions.SkiaSharp;

public static class SkiaSharpBitmapExtensions
{
    public static FFMpegResult AddAudio(this SKBitmap poster, string audio, string output)
    {
        var destination = Path.Combine(GlobalFFOptions.Current.TemporaryFilesFolder, $"{Guid.NewGuid()}.png");
        using (var fileStream = File.OpenWrite(destination))
        {
            poster.Encode(fileStream, SKEncodedImageFormat.Png, default); // PNG does not respect the quality parameter
        }

        try
        {
            return FFMpeg.PosterWithAudio(destination, audio, output).ProcessSynchronously();
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
