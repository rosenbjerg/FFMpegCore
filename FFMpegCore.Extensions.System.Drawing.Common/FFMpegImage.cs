using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FFMpegCore.Enums;
using FFMpegCore.Helpers;
using FFMpegCore.Pipes;

namespace FFMpegCore.Extensions.System.Drawing.Common
{
    public static class FFMpegImage
    {
        public static void ConversionSizeExceptionCheck(Image image)
            => FFMpegHelper.ConversionSizeExceptionCheck(image.Size.Width, image.Size.Height);

        /// <summary>
        /// Converts an image sequence to a video.
        /// </summary>
        /// <param name="output">Output video file.</param>
        /// <param name="frameRate">FPS</param>
        /// <param name="images">Image sequence collection</param>
        /// <returns>Output video information.</returns>
        public static bool JoinImageSequence(string output, double frameRate = 30, params ImageInfo[] images)
        {
            var tempFolderName = Path.Combine(GlobalFFOptions.Current.TemporaryFilesFolder, Guid.NewGuid().ToString());
            var temporaryImageFiles = images.Select((imageInfo, index) =>
            {
                using var image = Image.FromFile(imageInfo.FullName);
                FFMpegHelper.ConversionSizeExceptionCheck(image);
                var destinationPath = Path.Combine(tempFolderName, $"{index.ToString().PadLeft(9, '0')}{imageInfo.Extension}");
                Directory.CreateDirectory(tempFolderName);
                File.Copy(imageInfo.FullName, destinationPath);
                return destinationPath;
            }).ToArray();

            var firstImage = images.First();
            try
            {
                return FFMpegArguments
                    .FromFileInput(Path.Combine(tempFolderName, "%09d.png"), false)
                    .OutputToFile(output, true, options => options
                        .Resize(firstImage.Width, firstImage.Height)
                        .WithFramerate(frameRate))
                    .ProcessSynchronously();
            }
            finally
            {
                Cleanup(temporaryImageFiles);
                Directory.Delete(tempFolderName);
            }
        }
        /// <summary>
        ///     Adds a poster image to an audio file.
        /// </summary>
        /// <param name="image">Source image file.</param>
        /// <param name="audio">Source audio file.</param>
        /// <param name="output">Output video file.</param>
        /// <returns></returns>
        public static bool PosterWithAudio(string image, string audio, string output)
        {
            FFMpegHelper.ExtensionExceptionCheck(output, FileExtension.Mp4);
            using (var img = Image.FromFile(image))
                FFMpegHelper.ConversionSizeExceptionCheck(img);

            return FFMpegArguments
                .FromFileInput(image, false, options => options
                    .Loop(1))
                .AddFileInput(audio)
                .OutputToFile(output, true, options => options
                    .WithVideoCodec(VideoCodec.LibX264)
                    .CopyChannel()
                    .WithConstantRateFactor(21)
                    .WithAudioBitrate(AudioQuality.Normal)
                    .UsingShortest())
                .ProcessSynchronously();
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
        public static Bitmap Snapshot(string input, Size? size = null, TimeSpan? captureTime = null, int? streamIndex = null, int inputFileIndex = 0)
        {
            var source = FFProbe.Analyse(input);
            var (arguments, outputOptions) = BuildSnapshotArguments(input, source, size, captureTime, streamIndex, inputFileIndex);
            using var ms = new MemoryStream();

            arguments
                .OutputToPipe(new StreamPipeSink(ms), options => outputOptions(options
                    .ForceFormat("rawvideo")))
                .ProcessSynchronously();

            ms.Position = 0;
            using var bitmap = new Bitmap(ms);
            return bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
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
        public static async Task<Bitmap> SnapshotAsync(string input, Size? size = null, TimeSpan? captureTime = null, int? streamIndex = null, int inputFileIndex = 0)
        {
            var source = await FFProbe.AnalyseAsync(input).ConfigureAwait(false);
            var (arguments, outputOptions) = BuildSnapshotArguments(input, source, size, captureTime, streamIndex, inputFileIndex);
            using var ms = new MemoryStream();

            await arguments
                .OutputToPipe(new StreamPipeSink(ms), options => outputOptions(options
                    .ForceFormat("rawvideo")))
                .ProcessAsynchronously();

            ms.Position = 0;
            return new Bitmap(ms);
        }
        private static void Cleanup(IEnumerable<string> pathList)
        {
            foreach (var path in pathList)
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}