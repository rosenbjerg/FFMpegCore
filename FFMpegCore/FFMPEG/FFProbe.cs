using FFMpegCore.FFMPEG.Exceptions;
using FFMpegCore.Helpers;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Instances;
using FFMpegCore.FFMPEG.Argument;
using FFMpegCore.FFMPEG.Pipes;
using System.IO;

namespace FFMpegCore.FFMPEG
{
    public sealed class FFProbe
    {
        private readonly int _outputCapacity;
        static readonly double BITS_TO_MB = 1024 * 1024 * 8;
        private readonly string _ffprobePath;

        public FFProbe(int outputCapacity = int.MaxValue)
        {
            _outputCapacity = outputCapacity;
            FFProbeHelper.RootExceptionCheck(FFMpegOptions.Options.RootDirectory);
            _ffprobePath = FFMpegOptions.Options.FFProbeBinary;
        }

        /// <summary>
        ///     Probes the targeted video file and retrieves all available details.
        /// </summary>
        /// <param name="source">Source video file.</param>
        /// <returns>A video info object containing all details necessary.</returns>
        public VideoInfo ParseVideoInfo(string source)
        {
            return ParseVideoInfo(new VideoInfo(source));
        }
        /// <summary>
        ///     Probes the targeted video file asynchronously and retrieves all available details.
        /// </summary>
        /// <param name="source">Source video file.</param>
        /// <returns>A task for the video info object containing all details necessary.</returns>
        public Task<VideoInfo> ParseVideoInfoAsync(string source)
        {
            return ParseVideoInfoAsync(new VideoInfo(source));
        }

        /// <summary>
        ///     Probes the targeted video file and retrieves all available details.
        /// </summary>
        /// <param name="info">Source video file.</param>
        /// <returns>A video info object containing all details necessary.</returns>
        public VideoInfo ParseVideoInfo(VideoInfo info)
        {
            var instance = new Instance(_ffprobePath, BuildFFProbeArguments(info.FullName)) {DataBufferCapacity = _outputCapacity};
            instance.BlockUntilFinished();
            var output = string.Join("", instance.OutputData);
            return ParseVideoInfoInternal(info, output);
        }
        /// <summary>
        ///     Probes the targeted video file asynchronously and retrieves all available details.
        /// </summary>
        /// <param name="info">Source video file.</param>
        /// <returns>A video info object containing all details necessary.</returns>
        public async Task<VideoInfo> ParseVideoInfoAsync(VideoInfo info)
        {
            var instance = new Instance(_ffprobePath, BuildFFProbeArguments(info.FullName)) {DataBufferCapacity = _outputCapacity};
            await instance.FinishedRunning();
            var output = string.Join("", instance.OutputData);
            return ParseVideoInfoInternal(info, output);
        }

        /// <summary>
        ///     Probes the targeted video stream and retrieves all available details.
        /// </summary>
        /// <param name="stream">Encoded video stream.</param>
        /// <returns>A video info object containing all details necessary.</returns>
        public VideoInfo ParseVideoInfo(System.IO.Stream stream)
        {
            var info = new VideoInfo();
            var streamPipeSource = new StreamPipeDataWriter(stream);
            var pipeArgument = new InputPipeArgument(streamPipeSource);

            var instance = new Instance(_ffprobePath, BuildFFProbeArguments(pipeArgument.PipePath)) { DataBufferCapacity = _outputCapacity };
            pipeArgument.OpenPipe();

            var task = instance.FinishedRunning();
            try
            {
                pipeArgument.ProcessDataAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                pipeArgument.ClosePipe();
            }
            catch(IOException)
            {
            }
            finally
            {
                pipeArgument.ClosePipe();
            }
            var exitCode = task.ConfigureAwait(false).GetAwaiter().GetResult();

            if (exitCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Process, "FFProbe process returned exit status " + exitCode);
            var output = string.Join("", instance.OutputData);
            return ParseVideoInfoInternal(info, output);
        }

        /// <summary>
        ///     Probes the targeted video stream asynchronously and retrieves all available details.
        /// </summary>
        /// <param name="stream">Encoded video stream.</param>
        /// <returns>A video info object containing all details necessary.</returns>
        public async Task<VideoInfo> ParseVideoInfoAsync(System.IO.Stream stream)
        {
            var info = new VideoInfo();
            var streamPipeSource = new StreamPipeDataWriter(stream);
            var pipeArgument = new InputPipeArgument(streamPipeSource);

            var instance = new Instance(_ffprobePath, BuildFFProbeArguments(pipeArgument.PipePath)) { DataBufferCapacity = _outputCapacity };
            pipeArgument.OpenPipe();

            var task = instance.FinishedRunning();
            try
            {
                await pipeArgument.ProcessDataAsync();
                pipeArgument.ClosePipe();
            }
            catch (IOException)
            {
            }
            finally
            {
                pipeArgument.ClosePipe();
            }
            var exitCode = await task;

            var output = string.Join("", instance.OutputData);
            return ParseVideoInfoInternal(info, output);
        }

        private static string BuildFFProbeArguments(string fullPath) =>
            $"-v quiet -print_format json -show_streams \"{fullPath}\"";
        
        private VideoInfo ParseVideoInfoInternal(VideoInfo info, string probeOutput)
        {
            var metadata = JsonConvert.DeserializeObject<FFMpegStreamMetadata>(probeOutput);

            if (metadata.Streams == null || metadata.Streams.Count == 0)
            {
                throw new FFMpegException(FFMpegExceptionType.File, $"No video or audio streams could be detected. Source: ${info.FullName}");
            }
  
            var video = metadata.Streams.Find(s => s.CodecType == "video");
            var audio = metadata.Streams.Find(s => s.CodecType == "audio");

            var videoSize = 0d;
            var audioSize = 0d;

            var sDuration = (video ?? audio).Duration;
            var duration = TimeSpan.Zero;
            if (sDuration != null)
            {
                duration = TimeSpan.FromSeconds(double.TryParse(sDuration, NumberStyles.Any, CultureInfo.InvariantCulture, out var output) ? output : 0);
            }
            else
            {
                sDuration = (video ?? audio).Tags.Duration;
                if (sDuration != null)
                    TimeSpan.TryParse(sDuration.Remove(sDuration.LastIndexOf('.') + 8), CultureInfo.InvariantCulture, out duration); // TimeSpan fractions only allow up to 7 digits
            }
            info.Duration = duration;
            
            if (video != null)
            {
                var bitRate = Convert.ToDouble(video.BitRate, CultureInfo.InvariantCulture);
                var fr = video.FrameRate.Split('/');
                var commonDenominator = FFProbeHelper.Gcd(video.Width, video.Height);

                videoSize = bitRate * duration.TotalSeconds / BITS_TO_MB;

                info.VideoFormat = video.CodecName;
                info.Width = video.Width;
                info.Height = video.Height;
                info.FrameRate = Math.Round(
                    Convert.ToDouble(fr[0], CultureInfo.InvariantCulture) /
                    Convert.ToDouble(fr[1], CultureInfo.InvariantCulture),
                    3);
                info.Ratio = video.Width / commonDenominator + ":" + video.Height / commonDenominator;
            } else
            {
                info.VideoFormat = "none";
            }

            if (audio != null)
            {
                var bitRate = Convert.ToDouble(audio.BitRate, CultureInfo.InvariantCulture);
                info.AudioFormat = audio.CodecName;
                audioSize = bitRate * duration.TotalSeconds / BITS_TO_MB;
            } else
            {
                info.AudioFormat = "none";

            }

            info.Size = Math.Round(videoSize + audioSize, 2);

            return info;
        }

        internal FFMpegStreamMetadata GetMetadata(string path)
        {
            var instance = new Instance(_ffprobePath, BuildFFProbeArguments(path)) { DataBufferCapacity = _outputCapacity };
            instance.BlockUntilFinished();
            var output = string.Join("", instance.OutputData);
            return JsonConvert.DeserializeObject<FFMpegStreamMetadata>(output);
        }

        internal async Task<FFMpegStreamMetadata> GetMetadataAsync(string path)
        {
            var instance = new Instance(_ffprobePath, BuildFFProbeArguments(path)) { DataBufferCapacity = _outputCapacity };
            await instance.FinishedRunning();
            var output = string.Join("", instance.OutputData);
            return JsonConvert.DeserializeObject<FFMpegStreamMetadata>(output);
        }        
    }
}
