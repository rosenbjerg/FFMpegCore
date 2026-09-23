using System.Drawing;

namespace FFMpegCore.Extensions.System.Drawing.Common;

public static class SystemDrawingImageExtensions
{
    public static FFMpegResult AddAudio(this Image poster, string audio, string output, FFOptions? ffOptions = null)
    {
        var destination = SavePoster(poster, ffOptions ?? GlobalFFOptions.Current);
        try
        {
            return FFMpeg.PosterWithAudio(destination, audio, output).ProcessSynchronously(true, ffOptions);
        }
        finally
        {
            File.Delete(destination);
        }
    }

    public static async Task<FFMpegResult> AddAudioAsync(this Image poster, string audio, string output, FFOptions? ffOptions = null,
        CancellationToken cancellationToken = default)
    {
        var destination = SavePoster(poster, ffOptions ?? GlobalFFOptions.Current);
        try
        {
            return await FFMpeg.PosterWithAudio(destination, audio, output)
                .CancellableThrough(cancellationToken)
                .ProcessAsynchronously(true, ffOptions)
                .ConfigureAwait(false);
        }
        finally
        {
            File.Delete(destination);
        }
    }

    private static string SavePoster(Image poster, FFOptions ffOptions)
    {
        var destination = Path.Combine(ffOptions.TemporaryFilesFolder, $"{Guid.NewGuid()}.png");
        poster.Save(destination);
        return destination;
    }
}
