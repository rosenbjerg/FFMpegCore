using System.Drawing;
using FFMpegCore.Pipes;

namespace FFMpegCore.Extensions.System.Drawing.Common;

public static class SystemDrawingImage
{
    /// <summary>
    ///     Decodes a single frame of the input into an in-memory bitmap.
    /// </summary>
    /// <param name="input">Source video file.</param>
    /// <param name="size">Thumbnail size. If width or height is 0 or -1, it is computed from the other.</param>
    /// <param name="captureTime">Seek position the frame is taken from. Defaults to a third of the way in.</param>
    /// <param name="streamIndex">Selected video stream index.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    /// <returns>Bitmap with the requested snapshot.</returns>
    public static Bitmap Snapshot(string input, Size? size = null, TimeSpan? captureTime = null, int? streamIndex = null,
        FFOptions? ffOptions = null)
    {
        var source = FFProbe.Analyse(input, ffOptions);
        var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildSnapshotArguments(input, source, size, captureTime, streamIndex);
        using var ms = new MemoryStream();

        arguments
            .OutputToPipe(new StreamPipeSink(ms), options => outputOptions(options
                .ForceFormat("rawvideo")))
            .ProcessSynchronously(true, ffOptions);

        ms.Position = 0;
        using var bitmap = new Bitmap(ms);
        return bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
    }

    /// <summary>
    ///     Decodes a single frame of the input into an in-memory bitmap.
    /// </summary>
    /// <param name="input">Source video file.</param>
    /// <param name="size">Thumbnail size. If width or height is 0 or -1, it is computed from the other.</param>
    /// <param name="captureTime">Seek position the frame is taken from. Defaults to a third of the way in.</param>
    /// <param name="streamIndex">Selected video stream index.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Bitmap with the requested snapshot.</returns>
    public static async Task<Bitmap> SnapshotAsync(string input, Size? size = null, TimeSpan? captureTime = null, int? streamIndex = null,
        FFOptions? ffOptions = null, CancellationToken cancellationToken = default)
    {
        var source = await FFProbe.AnalyseAsync(input, ffOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
        var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildSnapshotArguments(input, source, size, captureTime, streamIndex);
        using var ms = new MemoryStream();

        await arguments
            .OutputToPipe(new StreamPipeSink(ms), options => outputOptions(options
                .ForceFormat("rawvideo")))
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously(true, ffOptions)
            .ConfigureAwait(false);

        ms.Position = 0;
        using var bitmap = new Bitmap(ms);
        return bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
    }
}
