using FFMpegCore.Enums;
using FFMpegCore.FFMPEG.Argument;
using FFMpegCore.FFMPEG.Enums;
using FFMpegCore.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace FFMpegCore.FFMPEG
{
    public static class FFMpeg
    {
        /// <summary>
        ///     Saves a 'png' thumbnail from the input video.
        /// </summary>
        /// <param name="source">Source video file.</param>
        /// <param name="output">Output video file</param>
        /// <param name="captureTime">Seek position where the thumbnail should be taken.</param>
        /// <param name="size">Thumbnail size. If width or height equal 0, the other will be computed automatically.</param>
        /// <param name="persistSnapshotOnFileSystem">By default, it deletes the created image on disk. If set to true, it won't delete the image</param>
        /// <returns>Bitmap with the requested snapshot.</returns>
        public static Bitmap Snapshot(MediaAnalysis source, string output, Size? size = null, TimeSpan? captureTime = null,
            bool persistSnapshotOnFileSystem = false)
        {
            if (captureTime == null)
                captureTime = TimeSpan.FromSeconds(source.Duration.TotalSeconds / 3);

            if (Path.GetExtension(output) != FileExtension.Png)
                output = Path.GetFileNameWithoutExtension(output) + FileExtension.Png;

            if (size == null || (size.Value.Height == 0 && size.Value.Width == 0))
                size = new Size(source.PrimaryVideoStream.Width, source.PrimaryVideoStream.Height);

            if (size.Value.Width != size.Value.Height)
            {
                if (size.Value.Width == 0)
                {
                    var ratio = source.PrimaryVideoStream.Width / (double)size.Value.Width;

                    size = new Size((int)(source.PrimaryVideoStream.Width * ratio), (int)(source.PrimaryVideoStream.Height * ratio));
                }

                if (size.Value.Height == 0)
                {
                    var ratio = source.PrimaryVideoStream.Height / (double)size.Value.Height;

                    size = new Size((int)(source.PrimaryVideoStream.Width * ratio), (int)(source.PrimaryVideoStream.Height * ratio));
                }
            }

            var success = FFMpegArguments
                .FromInputFiles(true, source.Path)
                .WithVideoCodec(VideoCodec.Png)
                .WithFrameOutputCount(1)
                .Resize(size)
                .Seek(captureTime)
                .OutputToFile(output)
                .ProcessSynchronously();
                
                
            if (!success)
                throw new OperationCanceledException("Could not take snapshot!");

            Bitmap result;
            using (var bmp = (Bitmap)Image.FromFile(output))
            {
                using var ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Png);
                result = new Bitmap(ms);
            }

            if (File.Exists(output) && !persistSnapshotOnFileSystem)
                File.Delete(output);

            return result;
        }

        /// <summary>
        /// Convert a video do a different format.
        /// </summary>
        /// <param name="source">Input video source.</param>
        /// <param name="output">Output information.</param>
        /// <param name="type">Target conversion video type.</param>
        /// <param name="speed">Conversion target speed/quality (faster speed = lower quality).</param>
        /// <param name="size">Video size.</param>
        /// <param name="audioQuality">Conversion target audio quality.</param>
        /// <param name="multithreaded">Is encoding multithreaded.</param>
        /// <returns>Output video information.</returns>
        public static bool Convert(
            MediaAnalysis source,
            string output,
            VideoType type = VideoType.Mp4,
            Speed speed = Speed.SuperFast,
            VideoSize size = VideoSize.Original,
            AudioQuality audioQuality = AudioQuality.Normal,
            bool multithreaded = false)
        {
            FFMpegHelper.ExtensionExceptionCheck(output, FileExtension.ForType(type));
            FFMpegHelper.ConversionSizeExceptionCheck(source);

            var scale = VideoSize.Original == size ? 1 : (double)source.PrimaryVideoStream.Height / (int)size;
            var outputSize = new Size((int)(source.PrimaryVideoStream.Width / scale), (int)(source.PrimaryVideoStream.Height / scale));

            if (outputSize.Width % 2 != 0)
                outputSize.Width += 1;

            return type switch
            {
                VideoType.Mp4 => FFMpegArguments
                    .FromInputFiles(true, source.Path)
                    .UsingMultithreading(multithreaded)
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithVideoBitrate(2400)
                    .Scale(outputSize)
                    .WithSpeedPreset(speed)
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithAudioBitrate(audioQuality)
                    .OutputToFile(output)
                    .ProcessSynchronously(),
                VideoType.Ogv => FFMpegArguments
                    .FromInputFiles(true, source.Path)
                    .UsingMultithreading(multithreaded)
                    .WithVideoCodec(VideoCodec.LibTheora)
                    .WithVideoBitrate(2400)
                    .Scale(outputSize)
                    .WithSpeedPreset(speed)
                    .WithAudioCodec(AudioCodec.LibVorbis)
                    .WithAudioBitrate(audioQuality)
                    .OutputToFile(output)
                    .ProcessSynchronously(),
                VideoType.Ts => FFMpegArguments
                    .FromInputFiles(true, source.Path)
                    .CopyChannel()
                    .WithBitStreamFilter(Channel.Video, Filter.H264_Mp4ToAnnexB)
                    .ForceFormat(VideoCodec.MpegTs)
                    .OutputToFile(output)
                    .ProcessSynchronously(),
                VideoType.WebM => FFMpegArguments
                    .FromInputFiles(true, source.Path)
                    .UsingMultithreading(multithreaded)
                    .WithVideoCodec(VideoCodec.LibVpx)
                    .WithVideoBitrate(2400)
                    .Scale(outputSize)
                    .WithSpeedPreset(speed)
                    .WithAudioCodec(AudioCodec.LibVorbis)
                    .WithAudioBitrate(audioQuality)
                    .OutputToFile(output)
                    .ProcessSynchronously(),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
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
            FFMpegHelper.ConversionSizeExceptionCheck(Image.FromFile(image));

            return FFMpegArguments
                .FromInputFiles(true, image, audio)
                .Loop(1)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithConstantRateFactor(21)
                .WithAudioBitrate(AudioQuality.Normal)
                .UsingShortest()
                .OutputToFile(output)
                .ProcessSynchronously();
        }

        /// <summary>
        ///     Joins a list of video files.
        /// </summary>
        /// <param name="output">Output video file.</param>
        /// <param name="videos">List of vides that need to be joined together.</param>
        /// <returns>Output video information.</returns>
        public static bool Join(string output, params MediaAnalysis[] videos)
        {
            var temporaryVideoParts = videos.Select(video =>
            {
                FFMpegHelper.ConversionSizeExceptionCheck(video);
                var destinationPath = video.Path.Replace(video.Extension, FileExtension.Ts);
                Convert(video, destinationPath, VideoType.Ts);
                return destinationPath;
            }).ToArray();

            try
            {
                return FFMpegArguments
                    .FromConcatenation(temporaryVideoParts)
                    .CopyChannel()
                    .WithBitStreamFilter(Channel.Audio, Filter.Aac_AdtstoAsc)
                    .OutputToFile(output)
                    .ProcessSynchronously();
            }
            finally
            {
                Cleanup(temporaryVideoParts);
            }
        }

        /// <summary>
        /// Converts an image sequence to a video.
        /// </summary>
        /// <param name="output">Output video file.</param>
        /// <param name="frameRate">FPS</param>
        /// <param name="images">Image sequence collection</param>
        /// <returns>Output video information.</returns>
        public static bool JoinImageSequence(string output, double frameRate = 30, params ImageInfo[] images)
        {
            var temporaryImageFiles = images.Select((image, index) =>
            {
                FFMpegHelper.ConversionSizeExceptionCheck(Image.FromFile(image.FullName));
                var destinationPath = image.FullName.Replace(image.Name, $"{index.ToString().PadLeft(9, '0')}{image.Extension}");
                File.Copy(image.FullName, destinationPath);
                return destinationPath;
            }).ToArray();

            var firstImage = images.First();
            try
            {
                return FFMpegArguments
                    .FromInputFiles(false, Path.Join(firstImage.Directory.FullName, "%09d.png"))
                    .WithVideoCodec(VideoCodec.LibX264)
                    .Resize(firstImage.Width, firstImage.Height)
                    .WithFrameOutputCount(images.Length)
                    .WithStartNumber(0)
                    .WithFramerate(frameRate)
                    .OutputToFile(output)
                    .ProcessSynchronously();
            }
            finally
            {
                Cleanup(temporaryImageFiles);
            }
        }

        /// <summary>
        ///     Records M3U8 streams to the specified output.
        /// </summary>
        /// <param name="uri">URI to pointing towards stream.</param>
        /// <param name="output">Output file</param>
        /// <returns>Success state.</returns>
        public static bool SaveM3U8Stream(Uri uri, string output)
        {
            FFMpegHelper.ExtensionExceptionCheck(output, FileExtension.Mp4);

            if (uri.Scheme == "http" || uri.Scheme == "https")
            {
                return FFMpegArguments
                    .FromInputFiles(false, uri)
                    .OutputToFile(output)
                    .ProcessSynchronously();
            }

            throw new ArgumentException($"Uri: {uri.AbsoluteUri}, does not point to a valid http(s) stream.");
        }

        /// <summary>
        ///     Strips a video file of audio.
        /// </summary>
        /// <param name="input">Input video file.</param>
        /// <param name="output">Output video file.</param>
        /// <returns></returns>
        public static bool Mute(string input, string output)
        {
            var source = FFProbe.Analyse(input);
            FFMpegHelper.ConversionSizeExceptionCheck(source);
            FFMpegHelper.ExtensionExceptionCheck(output, source.Extension);

            return FFMpegArguments
                .FromInputFiles(true, source.Path)
                .CopyChannel(Channel.Video)
                .DisableChannel(Channel.Audio)
                .OutputToFile(output)
                .ProcessSynchronously();
        }

        /// <summary>
        ///     Saves audio from a specific video file to disk.
        /// </summary>
        /// <param name="input">Source video file.</param>
        /// <param name="output">Output audio file.</param>
        /// <returns>Success state.</returns>
        public static bool ExtractAudio(string input, string output)
        {
            FFMpegHelper.ExtensionExceptionCheck(output, FileExtension.Mp3);

            return FFMpegArguments
                .FromInputFiles(true, input)
                .DisableChannel(Channel.Video)
                .OutputToFile(output)
                .ProcessSynchronously();
        }

        /// <summary>
        ///     Adds audio to a video file.
        /// </summary>
        /// <param name="input">Source video file.</param>
        /// <param name="inputAudio">Source audio file.</param>
        /// <param name="output">Output video file.</param>
        /// <param name="stopAtShortest">Indicates if the encoding should stop at the shortest input file.</param>
        /// <returns>Success state</returns>
        public static bool ReplaceAudio(string input, string inputAudio, string output, bool stopAtShortest = false)
        {
            var source = FFProbe.Analyse(input);
            FFMpegHelper.ConversionSizeExceptionCheck(source);
            FFMpegHelper.ExtensionExceptionCheck(output, source.Extension);

            return FFMpegArguments
                .FromInputFiles(true, source.Path, inputAudio)
                .CopyChannel()
                .WithAudioCodec(AudioCodec.Aac)
                .WithAudioBitrate(AudioQuality.Good)
                .UsingShortest(stopAtShortest)
                .OutputToFile(output)
                .ProcessSynchronously();
        }
        
        private static void Cleanup(IEnumerable<string> pathList)
        {
            foreach (var path in pathList)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

    }
}