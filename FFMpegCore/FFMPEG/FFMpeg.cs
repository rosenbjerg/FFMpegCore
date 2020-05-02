using FFMpegCore.Enums;
using FFMpegCore.FFMPEG.Argument;
using FFMpegCore.FFMPEG.Enums;
using FFMpegCore.FFMPEG.Exceptions;
using FFMpegCore.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Instances;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FFMpegCore.FFMPEG
{
    public delegate void ConversionHandler(double percentage);

    public class FFMpeg
    {
        IArgumentBuilder ArgumentBuilder { get; set; } = new FFArgumentBuilder();

        /// <summary>
        ///     Intializes the FFMPEG encoder.
        /// </summary>
        public FFMpeg() : base()
        {
            FFMpegHelper.RootExceptionCheck(FFMpegOptions.Options.RootDirectory);
            _ffmpegPath = FFMpegOptions.Options.FFmpegBinary;
        }

        /// <summary>
        /// Returns the percentage of the current conversion progress.
        /// </summary>
        public event ConversionHandler OnProgress;

        /// <summary>
        ///     Saves a 'png' thumbnail from the input video.
        /// </summary>
        /// <param name="source">Source video file.</param>
        /// <param name="output">Output video file</param>
        /// <param name="captureTime">Seek position where the thumbnail should be taken.</param>
        /// <param name="size">Thumbnail size. If width or height equal 0, the other will be computed automatically.</param>
        /// <param name="persistSnapshotOnFileSystem">By default, it deletes the created image on disk. If set to true, it won't delete the image</param>
        /// <returns>Bitmap with the requested snapshot.</returns>
        public Bitmap Snapshot(VideoInfo source, FileInfo output, Size? size = null, TimeSpan? captureTime = null,
            bool persistSnapshotOnFileSystem = false)
        {
            if (captureTime == null)
                captureTime = TimeSpan.FromSeconds(source.Duration.TotalSeconds / 3);

            if (output.Extension.ToLower() != FileExtension.Png)
                output = new FileInfo(output.FullName.Replace(output.Extension, FileExtension.Png));

            if (size == null || (size.Value.Height == 0 && size.Value.Width == 0))
            {
                size = new Size(source.Width, source.Height);
            }

            if (size.Value.Width != size.Value.Height)
            {
                if (size.Value.Width == 0)
                {
                    var ratio = source.Width / (double)size.Value.Width;

                    size = new Size((int)(source.Width * ratio), (int)(source.Height * ratio));
                }

                if (size.Value.Height == 0)
                {
                    var ratio = source.Height / (double)size.Value.Height;

                    size = new Size((int)(source.Width * ratio), (int)(source.Height * ratio));
                }
            }

            FFMpegHelper.ConversionExceptionCheck(source.ToFileInfo(), output);
            var container = new ArgumentContainer(
                new InputArgument(source),
                new VideoCodecArgument(VideoCodec.Png),
                new FrameOutputCountArgument(1),
                new SeekArgument(captureTime),
                new SizeArgument(size),
                new OutputArgument(output)
            );

            if (!RunProcess(container, output, false))
            {
                throw new OperationCanceledException("Could not take snapshot!");
            }

            output.Refresh();

            Bitmap result;
            using (var bmp = (Bitmap)Image.FromFile(output.FullName))
            {
                using var ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Png);
                result = new Bitmap(ms);
            }

            if (output.Exists && !persistSnapshotOnFileSystem)
            {
                output.Delete();
            }

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
        public VideoInfo Convert(
            VideoInfo source,
            FileInfo output,
            VideoType type = VideoType.Mp4,
            Speed speed = Speed.SuperFast,
            VideoSize size = VideoSize.Original,
            AudioQuality audioQuality = AudioQuality.Normal,
            bool multithreaded = false)
        {
            FFMpegHelper.ConversionExceptionCheck(source.ToFileInfo(), output);
            FFMpegHelper.ExtensionExceptionCheck(output, FileExtension.ForType(type));
            FFMpegHelper.ConversionSizeExceptionCheck(source);

            var scale = VideoSize.Original == size ? 1 : (double)source.Height / (int)size;
            var outputSize = new Size((int)(source.Width / scale), (int)(source.Height / scale));

            if (outputSize.Width % 2 != 0)
                outputSize.Width += 1;

            return type switch
            {
                VideoType.Mp4 => Convert(new ArgumentContainer(
                    new InputArgument(source),
                    new ThreadsArgument(multithreaded),
                    new ScaleArgument(outputSize),
                    new VideoCodecArgument(VideoCodec.LibX264, 2400),
                    new SpeedArgument(speed),
                    new AudioCodecArgument(AudioCodec.Aac),
                    new AudioBitrateArgument(audioQuality),
                    new OutputArgument(output))),
                VideoType.Ogv => Convert(new ArgumentContainer(
                    new InputArgument(source),
                    new ThreadsArgument(multithreaded),
                    new ScaleArgument(outputSize),
                    new VideoCodecArgument(VideoCodec.LibTheora, 2400),
                    new SpeedArgument(speed),
                    new AudioCodecArgument(AudioCodec.LibVorbis),
                    new AudioBitrateArgument(audioQuality),
                    new OutputArgument(output))),
                VideoType.Ts => Convert(new ArgumentContainer(
                    new InputArgument(source),
                    new CopyArgument(),
                    new BitStreamFilterArgument(Channel.Video, Filter.H264_Mp4ToAnnexB),
                    new ForceFormatArgument(VideoCodec.MpegTs),
                    new OutputArgument(output))),
                VideoType.WebM => Convert(new ArgumentContainer(
                    new InputArgument(source),
                    new ThreadsArgument(multithreaded),
                    new ScaleArgument(outputSize),
                    new VideoCodecArgument(VideoCodec.LibVpx, 2400),
                    new SpeedArgument(speed),
                    new AudioCodecArgument(AudioCodec.LibVorbis),
                    new AudioBitrateArgument(audioQuality),
                    new OutputArgument(output))),
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
        public VideoInfo PosterWithAudio(FileInfo image, FileInfo audio, FileInfo output)
        {
            FFMpegHelper.InputsExistExceptionCheck(image, audio);
            FFMpegHelper.ExtensionExceptionCheck(output, FileExtension.Mp4);
            FFMpegHelper.ConversionSizeExceptionCheck(Image.FromFile(image.FullName));

            var container = new ArgumentContainer(
                new InputArgument(image.FullName, audio.FullName),
                new LoopArgument(1),
                new VideoCodecArgument(VideoCodec.LibX264, 2400),
                new AudioCodecArgument(AudioCodec.Aac),
                new AudioBitrateArgument(AudioQuality.Normal),
                new ShortestArgument(true),
                new OutputArgument(output)
            );
            if (!RunProcess(container, output, false))
            {
                throw new FFMpegException(FFMpegExceptionType.Operation,
                    "An error occured while adding the audio file to the image.");
            }

            return new VideoInfo(output);
        }

        /// <summary>
        ///     Joins a list of video files.
        /// </summary>
        /// <param name="output">Output video file.</param>
        /// <param name="videos">List of vides that need to be joined together.</param>
        /// <returns>Output video information.</returns>
        public VideoInfo Join(FileInfo output, params VideoInfo[] videos)
        {
            FFMpegHelper.OutputExistsExceptionCheck(output);
            FFMpegHelper.InputsExistExceptionCheck(videos.Select(video => video.ToFileInfo()).ToArray());

            var temporaryVideoParts = videos.Select(video =>
            {
                FFMpegHelper.ConversionSizeExceptionCheck(video);
                var destinationPath = video.FullName.Replace(video.Extension, FileExtension.Ts);
                Convert(video, new FileInfo(destinationPath), VideoType.Ts);
                return destinationPath;
            }).ToList();

            try
            {
                return Convert(new ArgumentContainer(
                    new ConcatArgument(temporaryVideoParts),
                    new CopyArgument(),
                    new BitStreamFilterArgument(Channel.Audio, Filter.Aac_AdtstoAsc),
                    new OutputArgument(output)
                ));
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
        public VideoInfo JoinImageSequence(FileInfo output, double frameRate = 30, params ImageInfo[] images)
        {
            var temporaryImageFiles = images.Select((image, index) =>
            {
                FFMpegHelper.ConversionSizeExceptionCheck(Image.FromFile(image.FullName));
                var destinationPath =
                    image.FullName.Replace(image.Name, $"{index.ToString().PadLeft(9, '0')}{image.Extension}");
                File.Copy(image.FullName, destinationPath);

                return destinationPath;
            }).ToList();

            var firstImage = images.First();

            var container = new ArgumentContainer(
                new FrameRateArgument(frameRate),
                new SizeArgument(firstImage.Width, firstImage.Height),
                new StartNumberArgument(0),
                new InputArgument($"{firstImage.Directory}{Path.DirectorySeparatorChar}%09d.png"),
                new FrameOutputCountArgument(images.Length),
                new VideoCodecArgument(VideoCodec.LibX264),
                new OutputArgument(output)
            );

            try
            {
                if (!RunProcess(container, output, false))
                {
                    throw new FFMpegException(FFMpegExceptionType.Operation,
                        "Could not join the provided image sequence.");
                }

                return new VideoInfo(output);
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
        public VideoInfo SaveM3U8Stream(Uri uri, FileInfo output)
        {
            FFMpegHelper.ExtensionExceptionCheck(output, FileExtension.Mp4);

            if (uri.Scheme == "http" || uri.Scheme == "https")
            {
                return Convert(new ArgumentContainer(
                    new InputArgument(uri),
                    new OutputArgument(output)
                ));
            }

            throw new ArgumentException($"Uri: {uri.AbsoluteUri}, does not point to a valid http(s) stream.");
        }

        /// <summary>
        ///     Strips a video file of audio.
        /// </summary>
        /// <param name="source">Source video file.</param>
        /// <param name="output">Output video file.</param>
        /// <returns></returns>
        public VideoInfo Mute(VideoInfo source, FileInfo output)
        {
            FFMpegHelper.ConversionExceptionCheck(source.ToFileInfo(), output);
            FFMpegHelper.ConversionSizeExceptionCheck(source);
            FFMpegHelper.ExtensionExceptionCheck(output, source.Extension);

            return Convert(new ArgumentContainer(
                new InputArgument(source),
                new CopyArgument(Channel.Video),
                new DisableChannelArgument(Channel.Audio),
                new OutputArgument(output)
            ));
        }

        /// <summary>
        ///     Saves audio from a specific video file to disk.
        /// </summary>
        /// <param name="source">Source video file.</param>
        /// <param name="output">Output audio file.</param>
        /// <returns>Success state.</returns>
        public FileInfo ExtractAudio(VideoInfo source, FileInfo output)
        {
            FFMpegHelper.ConversionExceptionCheck(source.ToFileInfo(), output);
            FFMpegHelper.ExtensionExceptionCheck(output, FileExtension.Mp3);

            var container = new ArgumentContainer(
                new InputArgument(source),
                new DisableChannelArgument(Channel.Video),
                new OutputArgument(output)
            );

            if (!RunProcess(container, output, false))
            {
                throw new FFMpegException(FFMpegExceptionType.Operation,
                    "Could not extract the audio from the requested video.");
            }

            output.Refresh();

            return output;
        }

        /// <summary>
        ///     Adds audio to a video file.
        /// </summary>
        /// <param name="source">Source video file.</param>
        /// <param name="audio">Source audio file.</param>
        /// <param name="output">Output video file.</param>
        /// <param name="stopAtShortest">Indicates if the encoding should stop at the shortest input file.</param>
        /// <returns>Success state</returns>
        public VideoInfo ReplaceAudio(VideoInfo source, FileInfo audio, FileInfo output, bool stopAtShortest = false)
        {
            FFMpegHelper.ConversionExceptionCheck(source.ToFileInfo(), output);
            FFMpegHelper.InputsExistExceptionCheck(audio);
            FFMpegHelper.ConversionSizeExceptionCheck(source);
            FFMpegHelper.ExtensionExceptionCheck(output, source.Extension);

            return Convert(new ArgumentContainer(
                new InputArgument(source.FullName, audio.FullName),
                new CopyArgument(),
                new AudioCodecArgument(AudioCodec.Aac),
                new AudioBitrateArgument(AudioQuality.Hd),
                new ShortestArgument(stopAtShortest),
                new OutputArgument(output)
            ));
        }

        public VideoInfo Convert(ArgumentContainer arguments, bool skipExistsCheck = false)
        {
            var (sources, output) = GetInputOutput(arguments);
            if (sources != null)
                _totalTime = TimeSpan.FromSeconds(sources.Sum(source => source.Duration.TotalSeconds));

            if (!RunProcess(arguments, output, skipExistsCheck))
                throw new FFMpegException(FFMpegExceptionType.Conversion, "Could not process file without error");

            _totalTime = TimeSpan.MinValue;

            return output != null && output.Exists ? new VideoInfo(output) : null;
        }
        public async Task<VideoInfo> ConvertAsync(ArgumentContainer arguments, bool skipExistsCheck = false)
        {
            var (sources, output) = GetInputOutput(arguments);
            if (sources != null)
                _totalTime = TimeSpan.FromSeconds(sources.Sum(source => source.Duration.TotalSeconds));

            if (!await RunProcessAsync(arguments, output, skipExistsCheck))
                throw new FFMpegException(FFMpegExceptionType.Conversion, "Could not process file without error");

            _totalTime = TimeSpan.MinValue;

            return output != null && output.Exists ? new VideoInfo(output) : null;
        }

        private static (VideoInfo[] Input, FileInfo Output) GetInputOutput(ArgumentContainer arguments)
        {
            FileInfo output;
            if (arguments.TryGetArgument<OutputArgument>(out var outputArg))
                output = outputArg.GetAsFileInfo();
            else if (arguments.TryGetArgument<OutputPipeArgument>(out var outputPipeArg))
                output = null;
            else
                throw new FFMpegException(FFMpegExceptionType.Operation, "No output argument found");

            VideoInfo[] sources;
            if (arguments.TryGetArgument<InputArgument>(out var input))
                sources = input.GetAsVideoInfo();
            else if (arguments.TryGetArgument<ConcatArgument>(out var concat))
                sources = concat.GetAsVideoInfo();
            else if (arguments.TryGetArgument<InputPipeArgument>(out var pipe))
                sources = null;
            else
                throw new FFMpegException(FFMpegExceptionType.Operation, "No input or concat argument found");
            return (sources, output);
        }

        /// <summary>
        ///     Returns true if the associated process is still alive/running.
        /// </summary>
        public bool IsWorking => _instance.Started;

        /// <summary>
        ///     Stops any current job that FFMpeg is running.
        /// </summary>
        public void Stop()
        {
            if (IsWorking)
            {
                _instance.SendInput("q").Wait();
            }
        }

        #region Private Members & Methods

        private readonly string _ffmpegPath;
        private TimeSpan _totalTime;

        private bool RunProcess(ArgumentContainer container, FileInfo output, bool skipExistsCheck)
        {
            _instance?.Dispose();
            var arguments = ArgumentBuilder.BuildArguments(container);
            var exitCode = -1;

            if (container.TryGetArgument<InputPipeArgument>(out var inputPipeArgument))
            {
                inputPipeArgument.OpenPipe();
            }
            if (container.TryGetArgument<OutputPipeArgument>(out var outputPipeArgument))
            {
                outputPipeArgument.OpenPipe();
            }


            _instance = new Instance(_ffmpegPath, arguments);
            _instance.DataReceived += OutputData;

            if (inputPipeArgument != null || outputPipeArgument != null)
            {
                try
                {
                    using (var tokenSource = new CancellationTokenSource())
                    {
                        var concurrentTasks = new List<Task>();
                        concurrentTasks.Add(_instance.FinishedRunning()
                            .ContinueWith((t =>
                            {
                                exitCode = t.Result;
                                if (exitCode != 0)
                                    tokenSource.Cancel();
                            })));
                        if (inputPipeArgument != null)
                            concurrentTasks.Add(inputPipeArgument.ProcessDataAsync(tokenSource.Token)
                                .ContinueWith((t) =>
                                {
                                    inputPipeArgument.ClosePipe();
                                    if (t.Exception != null)
                                        throw t.Exception;
                                }));
                        if (outputPipeArgument != null)
                            concurrentTasks.Add(outputPipeArgument.ProcessDataAsync(tokenSource.Token)
                                .ContinueWith((t) =>
                                {
                                    outputPipeArgument.ClosePipe();
                                    if (t.Exception != null)
                                        throw t.Exception;
                                }));

                        Task.WaitAll(concurrentTasks.ToArray()/*, tokenSource.Token*/);
                    }
                }
                catch (Exception ex)
                {
                    inputPipeArgument?.ClosePipe();
                    outputPipeArgument?.ClosePipe();
                    throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\n", _instance.ErrorData), ex);
                }
            }
            else
            {
                exitCode = _instance.BlockUntilFinished();
            }

            if(exitCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\n", _instance.ErrorData));

            if (outputPipeArgument == null && !skipExistsCheck && (!File.Exists(output.FullName) || new FileInfo(output.FullName).Length == 0))
                throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\n", _instance.ErrorData));

            return exitCode == 0;
        }
        private async Task<bool> RunProcessAsync(ArgumentContainer container, FileInfo output, bool skipExistsCheck)
        {
            _instance?.Dispose();
            var arguments = ArgumentBuilder.BuildArguments(container);
            var exitCode = -1;

            if (container.TryGetArgument<InputPipeArgument>(out var inputPipeArgument))
            {
                inputPipeArgument.OpenPipe();
            }
            if (container.TryGetArgument<OutputPipeArgument>(out var outputPipeArgument))
            {
                outputPipeArgument.OpenPipe();
            }


            _instance = new Instance(_ffmpegPath, arguments);
            _instance.DataReceived += OutputData;

            if (inputPipeArgument != null || outputPipeArgument != null)
            {
                try
                {
                    using (var tokenSource = new CancellationTokenSource())
                    {
                        var concurrentTasks = new List<Task>();
                        concurrentTasks.Add(_instance.FinishedRunning()
                            .ContinueWith((t =>
                            {
                                exitCode = t.Result;
                                if (exitCode != 0)
                                    tokenSource.Cancel();
                            })));
                        if (inputPipeArgument != null)
                            concurrentTasks.Add(inputPipeArgument.ProcessDataAsync(tokenSource.Token)
                                .ContinueWith((t) =>
                                {
                                    inputPipeArgument.ClosePipe();
                                    if (t.Exception != null)
                                        throw t.Exception;
                                }));
                        if (outputPipeArgument != null)
                            concurrentTasks.Add(outputPipeArgument.ProcessDataAsync(tokenSource.Token)
                                .ContinueWith((t) =>
                                {
                                    outputPipeArgument.ClosePipe();
                                    if (t.Exception != null)
                                        throw t.Exception;
                                }));

                        await Task.WhenAll(concurrentTasks);
                    }
                }
                catch (Exception ex)
                {
                    inputPipeArgument?.ClosePipe();
                    outputPipeArgument?.ClosePipe();
                    throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\n", _instance.ErrorData), ex);
                }
            }
            else
            {
                exitCode = await _instance.FinishedRunning();
            }

            if (exitCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\n", _instance.ErrorData));

            if (outputPipeArgument == null && !skipExistsCheck && (!File.Exists(output.FullName) || new FileInfo(output.FullName).Length == 0))
                throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\n", _instance.ErrorData));

            return exitCode == 0;
        }

        private void Cleanup(IEnumerable<string> pathList)
        {
            foreach (var path in pathList)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        private static readonly Regex ProgressRegex = new Regex(@"time=(\d\d:\d\d:\d\d.\d\d?)", RegexOptions.Compiled);
        private Instance _instance;

        private void OutputData(object sender, (DataType Type, string Data) msg)
        {
#if DEBUG
            Trace.WriteLine(msg.Data);
#endif
            if (OnProgress == null) return;

            var match = ProgressRegex.Match(msg.Data);
            if (!match.Success) return;

            var processed = TimeSpan.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var percentage = Math.Round(processed.TotalSeconds / _totalTime.TotalSeconds * 100, 2);
            OnProgress(percentage);
        }

        #endregion
    }
}