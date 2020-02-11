using FFMpegCore.FFMPEG.Exceptions;
using FFMpegCore.Helpers;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG
{
    public sealed class FFProbe : FFBase
    {
        static readonly double BITS_TO_MB = 1024 * 1024 * 8;

        public FFProbe(): base()
        {
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
            var output = RunProcess(BuildFFProbeArguments(info));
            return ParseVideoInfoInternal(info, output);
        }
        /// <summary>
        ///     Probes the targeted video file asynchronously and retrieves all available details.
        /// </summary>
        /// <param name="info">Source video file.</param>
        /// <returns>A video info object containing all details necessary.</returns>
        public async Task<VideoInfo> ParseVideoInfoAsync(VideoInfo info)
        {
            var output = await RunProcessAsync(_ffprobePath, BuildFFProbeArguments(info));
            return ParseVideoInfoInternal(info, output);
        }

        private static string BuildFFProbeArguments(VideoInfo info) =>
            $"-v quiet -print_format json -show_streams \"{info.FullName}\"";
        
        private VideoInfo ParseVideoInfoInternal(VideoInfo info, string probeOutput)
        {
            var metadata = JsonConvert.DeserializeObject<FFMpegStreamMetadata>(probeOutput);

            if (metadata.Streams == null || metadata.Streams.Count == 0)
            {
                throw new FFMpegException(FFMpegExceptionType.File, $"No video or audio streams could be detected. Source: ${info.FullName}");
            }
  
            var video = metadata.Streams.Find(s => s.CodecType == "video");
            var audio = metadata.Streams.Find(s => s.CodecType == "audio");

            double videoSize = 0d;
            double audioSize = 0d;

            var duration = TimeSpan.FromSeconds(double.TryParse((video ?? audio).Duration, NumberStyles.Any, CultureInfo.InvariantCulture, out var output) ? output : 0);
            info.Duration = duration.Subtract(TimeSpan.FromMilliseconds(duration.Milliseconds));

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

        #region Private Members & Methods

        private readonly string _ffprobePath;

        private string RunProcess(string args)
        {
            CreateProcess(args, _ffprobePath, rStandardOutput: true);

            string output;

            try
            {
                Process.Start();
                output = Process.StandardOutput.ReadToEnd();
            }
            catch (Exception)
            {
                output = "";
            }
            finally
            {
                Process.WaitForExit();
                Process.Close();
            }

            return output;
        }

        #endregion
    }
}
