using FFMpegCore.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace FFMpegCore.FFMPEG
{
    public sealed class FFProbe : FFBase
    {
        public FFProbe()
        {
            FFProbeHelper.RootExceptionCheck(ConfiguredRoot);

            var target = Environment.Is64BitProcess ? "x64" : "x86";

            _ffprobePath = ConfiguredRoot + $"\\{target}\\ffprobe.exe";
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
        ///     Probes the targeted video file and retrieves all available details.
        /// </summary>
        /// <param name="info">Source video file.</param>
        /// <returns>A video info object containing all details necessary.</returns>
        public VideoInfo ParseVideoInfo(VideoInfo info)
        {
            var jsonOutput =
                RunProcess($"-v quiet -print_format json -show_streams \"{info.FullName}\"");

            var metadata = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonOutput);
            int videoIndex = metadata["streams"][0]["codec_type"] == "video" ? 0 : 1,
                audioIndex = 1 - videoIndex;

            var bitRate = Convert.ToDouble(metadata["streams"][videoIndex]["bit_rate"], CultureInfo.InvariantCulture);
                       
            try
            {
                var duration = Convert.ToDouble(metadata["streams"][videoIndex]["duration"], CultureInfo.InvariantCulture);
                info.Duration = TimeSpan.FromSeconds(duration);
                info.Duration = info.Duration.Subtract(TimeSpan.FromMilliseconds(info.Duration.Milliseconds));
            }
            catch (Exception)
            {
                info.Duration = TimeSpan.FromSeconds(0);
            }


            // Get video size in megabytes
            double videoSize = 0,
                   audioSize = 0;

            try
            {
                info.VideoFormat = metadata["streams"][videoIndex]["codec_name"];
                videoSize = bitRate * info.Duration / 8388608;
            }
            catch (Exception)
            {
                info.VideoFormat = "none";
            }

            // Get audio format - wrap for exceptions if the video has no audio
            try
            {
                info.AudioFormat = metadata["streams"][audioIndex]["codec_name"];
                audioSize = bitRate *  info.Duration / 8388608;
            }
            catch (Exception)
            {
                info.AudioFormat = "none";
            }

            // Get video format


            // Get video width
            info.Width = metadata["streams"][videoIndex]["width"];

            // Get video height
            info.Height = metadata["streams"][videoIndex]["height"];

            info.Size = Math.Round(videoSize + audioSize, 2);

            // Get video aspect ratio
            var cd = FFProbeHelper.Gcd(info.Width, info.Height);
            info.Ratio = info.Width / cd + ":" + info.Height / cd;

            // Get video framerate
            var fr = ((string)metadata["streams"][videoIndex]["r_frame_rate"]).Split('/');
            info.FrameRate = Math.Round(
                Convert.ToDouble(fr[0], CultureInfo.InvariantCulture) /
                Convert.ToDouble(fr[1], CultureInfo.InvariantCulture),
                3);

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