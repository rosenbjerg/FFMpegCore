using System.Drawing;
using FFMpegCore.Pipes;
using SkiaSharp;

namespace FFMpegCore.Extensions.System.Drawing.Common
{
    public static class FFMpegImage
    {
        /// <summary>
        ///     Saves a 'png' thumbnail to an in-memory bitmap
        /// </summary>
        /// <param name="input">Source video file.</param>
        /// <param name="captureTime">Seek position where the thumbnail should be taken.</param>
        /// <param name="size">Thumbnail size. If width or height equal 0, the other will be computed automatically.</param>
        /// <param name="streamIndex">Selected video stream index.</param>
        /// <param name="inputFileIndex">Input file index</param>
        /// <returns>Bitmap with the requested snapshot.</returns>
        public static SKBitmap Snapshot(string input, Size? size = null, TimeSpan? captureTime = null, int? streamIndex = null, int inputFileIndex = 0)
        {
            var source = FFProbe.Analyse(input);
            var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildSnapshotArguments(input, source, size, captureTime, streamIndex, inputFileIndex);
            using var ms = new MemoryStream();

            arguments
                .OutputToPipe(new StreamPipeSink(ms), options => outputOptions(options
                    .ForceFormat("rawvideo")))
                .ProcessSynchronously();

            ms.Position = 0;
            using var bitmap = SKBitmap.Decode(ms);
            return bitmap.Copy();
        }
        /// <summary>
        ///     Saves a 'png' thumbnail to an in-memory bitmap
        /// </summary>
        /// <param name="input">Source video file.</param>
        /// <param name="captureTime">Seek position where the thumbnail should be taken.</param>
        /// <param name="size">Thumbnail size. If width or height equal 0, the other will be computed automatically.</param>
        /// <param name="streamIndex">Selected video stream index.</param>
        /// <param name="inputFileIndex">Input file index</param>
        /// <returns>Bitmap with the requested snapshot.</returns>
        public static async Task<SKBitmap> SnapshotAsync(string input, Size? size = null, TimeSpan? captureTime = null, int? streamIndex = null, int inputFileIndex = 0)
        {
            var source = await FFProbe.AnalyseAsync(input).ConfigureAwait(false);
            var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildSnapshotArguments(input, source, size, captureTime, streamIndex, inputFileIndex);
            using var ms = new MemoryStream();

            await arguments
                .OutputToPipe(new StreamPipeSink(ms), options => outputOptions(options
                    .ForceFormat("rawvideo")))
                .ProcessAsynchronously();

            ms.Position = 0;
            return SKBitmap.Decode(ms);
        }
    }
}
