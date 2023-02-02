using System.Drawing;
using FFMpegCore.Extend;

namespace FFMpegCore.Arguments
{
    public class SubtitleHardBurnArgument : IVideoFilterArgument
    {
        private readonly SubtitleHardBurnOptions _subtitleHardBurnOptions;

        public SubtitleHardBurnArgument(SubtitleHardBurnOptions subtitleHardBurnOptions)
        {
            _subtitleHardBurnOptions = subtitleHardBurnOptions;
        }

        public string Key => "subtitles";

        public string Value => _subtitleHardBurnOptions.TextInternal;
    }

    public class SubtitleHardBurnOptions
    {
        private readonly string _subtitle;

        public readonly Dictionary<string, string> Parameters = new();

        /// <summary>
        ///     Create a new <see cref="SubtitleHardBurnOptions"/> using a provided subtitle file or a video file
        ///     containing one.
        /// </summary> 
        /// <param name="subtitlePath"></param>
        /// <returns></returns>
        /// <remarks>Only support .srt and .ass files, and subrip and ssa subtitle streams</remarks>
        public static SubtitleHardBurnOptions Create(string subtitlePath)
        {
            return new SubtitleHardBurnOptions(subtitlePath);
        }

        private SubtitleHardBurnOptions(string subtitle)
        {
            _subtitle = subtitle;
        }

        /// <summary>
        ///     Specify the size of the original video, the video for which the ASS file was composed.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public SubtitleHardBurnOptions SetOriginalSize(int width, int height)
        {
            return WithParameter("original_size", $"{width}x{height}");
        }

        /// <summary>
        ///     Specify the size of the original video, the video for which the ASS file was composed.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public SubtitleHardBurnOptions SetOriginalSize(Size size)
        {
            return SetOriginalSize(size.Width, size.Height);
        }

        /// <summary>
        ///     Set subtitles stream index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <remarks>
        ///     Used when the provided subtitle is an stream of a video file (ex. .mkv) with multiple subtitles.
        ///     Represent the index of the subtitle not the stream, them the first subtitle index is 0 and second is 1
        /// </remarks>
        public SubtitleHardBurnOptions SetSubtitleIndex(int index)
        {
            return WithParameter("stream_index", index.ToString());
        }

        /// <summary>
        ///     Set subtitles input character encoding. Only useful if not UTF-8
        /// </summary>
        /// <param name="encode">Charset encoding</param>
        /// <returns></returns>
        public SubtitleHardBurnOptions SetCharacterEncoding(string encode)
        {
            return WithParameter("charenc", encode);
        }

        /// <summary>
        ///     Override default style or script info parameters of the subtitles
        /// </summary>
        /// <param name="styleOptions"></param>
        /// <returns></returns>
        public SubtitleHardBurnOptions WithStyle(StyleOptions styleOptions)
        {
            return WithParameter("force_style", styleOptions.TextInternal);
        }

        public SubtitleHardBurnOptions WithParameter(string key, string value)
        {
            Parameters.Add(key, value);
            return this;
        }

        internal string TextInternal => string
            .Join(":", new[] { StringExtensions.EncloseInQuotes(StringExtensions.ToFFmpegLibavfilterPath(_subtitle)) }
            .Concat(Parameters.Select(parameter => parameter.FormatArgumentPair(enclose: true))));
    }

    public class StyleOptions
    {
        public readonly Dictionary<string, string> Parameters = new();

        public static StyleOptions Create()
        {
            return new StyleOptions();
        }

        /// <summary>
        ///     Used to override default style or script info parameters of the subtitles. It accepts ASS style format
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public StyleOptions WithParameter(string key, string value)
        {
            Parameters.Add(key, value);
            return this;
        }

        internal string TextInternal => string.Join(",", Parameters.Select(parameter => parameter.FormatArgumentPair(enclose: false)));
    }
}
