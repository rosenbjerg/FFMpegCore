using SkiaSharp;

namespace FFMpegCore.Extensions.SkiaSharp;

public static class SkiaSharpBitmapExtensions
{
    public static FFMpegResult AddAudio(this SKBitmap poster, string audio, string output, FFOptions? ffOptions = null)
    {
        var destination = SavePoster(poster, ffOptions ?? GlobalFFOptions.Current);
        try
        {
            return FFMpeg.PosterWithAudio(destination, audio, output, ffOptions).ProcessSynchronously();
        }
        finally
        {
            File.Delete(destination);
        }
    }

    public static async Task<FFMpegResult> AddAudioAsync(this SKBitmap poster, string audio, string output, FFOptions? ffOptions = null,
        CancellationToken cancellationToken = default)
    {
        var destination = SavePoster(poster, ffOptions ?? GlobalFFOptions.Current);
        try
        {
            return await FFMpeg.PosterWithAudio(destination, audio, output, ffOptions)
                .CancellableThrough(cancellationToken)
                .ProcessAsynchronously()
                .ConfigureAwait(false);
        }
        finally
        {
            File.Delete(destination);
        }
    }

    private static string SavePoster(SKBitmap poster, FFOptions ffOptions)
    {
        var destination = Path.Combine(ffOptions.TemporaryFilesFolder, $"{Guid.NewGuid()}.png");
        using var fileStream = File.OpenWrite(destination);
        poster.Encode(fileStream, SKEncodedImageFormat.Png, default);
        return destination;
    }
}
