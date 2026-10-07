using System.Drawing;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using SkiaSharp;

namespace FFMpegCore.Extensions.SkiaSharp;

public static class SkiaSharpImage
{
    /// <summary>
    ///     Decodes a single frame of the input into an in-memory bitmap.
    /// </summary>
    /// <param name="input">Source video file.</param>
    /// <param name="size">Thumbnail size. If width or height is 0 or -1, it is computed from the other.</param>
    /// <param name="captureTime">Seek position the frame is taken from. Defaults to a third of the way in.</param>
    /// <param name="streamIndex">Index of the video stream to take, as in <see cref="MediaStream.Index" />. Defaults to the first video stream.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    /// <returns>Bitmap with the requested snapshot.</returns>
    public static SKBitmap Snapshot(string input, Size? size = null, TimeSpan? captureTime = null, int? streamIndex = null,
        FFOptions? ffOptions = null)
    {
        var source = FFProbe.Analyse(input, ffOptions);
        var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildSnapshotArguments(input, source, size, captureTime, streamIndex);
        using var ms = new MemoryStream();

        arguments
            .OutputToPipe(new StreamPipeSink(ms), options => outputOptions(options
                .ForceFormat(ContainerFormats.RawVideo)))
            .ProcessSynchronously(true, ffOptions);

        ms.Position = 0;
        return SKBitmap.Decode(ms);
    }

    /// <summary>
    ///     Decodes a single frame of the input into an in-memory bitmap.
    /// </summary>
    /// <param name="input">Source video file.</param>
    /// <param name="size">Thumbnail size. If width or height is 0 or -1, it is computed from the other.</param>
    /// <param name="captureTime">Seek position the frame is taken from. Defaults to a third of the way in.</param>
    /// <param name="streamIndex">Index of the video stream to take, as in <see cref="MediaStream.Index" />. Defaults to the first video stream.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Bitmap with the requested snapshot.</returns>
    public static async Task<SKBitmap> SnapshotAsync(string input, Size? size = null, TimeSpan? captureTime = null, int? streamIndex = null,
        FFOptions? ffOptions = null, CancellationToken cancellationToken = default)
    {
        var source = await FFProbe.AnalyseAsync(input, ffOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
        var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildSnapshotArguments(input, source, size, captureTime, streamIndex);
        using var ms = new MemoryStream();

        await arguments
            .OutputToPipe(new StreamPipeSink(ms), options => outputOptions(options
                .ForceFormat(ContainerFormats.RawVideo)))
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously(true, ffOptions)
            .ConfigureAwait(false);

        ms.Position = 0;
        return SKBitmap.Decode(ms);
    }
}
