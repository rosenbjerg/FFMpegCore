using FFMpegCore.Extend;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

        public readonly Dictionary<string, string> Parameters = new Dictionary<string, string>();

        public static SubtitleHardBurnOptions Create(string subtitlePath)
        {
            return new SubtitleHardBurnOptions(subtitlePath);
        }

        private SubtitleHardBurnOptions(string subtitle)
        {
            _subtitle = subtitle;
        }

        public SubtitleHardBurnOptions SetOriginalSize(int width, int height)
        {
            return WithParameter("original_size", $"{width}x{height}");
        }

        public SubtitleHardBurnOptions SetOriginalSize(Size size)
        {
            return SetOriginalSize(size.Width, size.Height);
        }

        public SubtitleHardBurnOptions SetSubtitleIndex(int index)
        {
            return WithParameter("si", index.ToString());
        }

        public SubtitleHardBurnOptions SetCharacterEncoding(string encode)
        {
            return WithParameter("charenc", encode);
        }

        public SubtitleHardBurnOptions WithStyle(StyleOptions styleOptions)
        {
            return WithParameter("force_style", styleOptions.TextInternal);
        }

        public SubtitleHardBurnOptions WithParameter(string key, string value)
        {
            Parameters.Add(key, value);
            return this;
        }

        internal string TextInternal => string.Join(":", new[] { _subtitle.EncloseIfContainsSpace() }.Concat(Parameters.Select(parameter => parameter.FormatArgumentPair(enclose: true))));
    }

    public class StyleOptions
    {
        public readonly Dictionary<string, string> Parameters = new Dictionary<string, string>();

        public static StyleOptions Create()
        {
            return new StyleOptions();
        }

        public StyleOptions WithParameter(string key, string value)
        {
            Parameters.Add(key, value);
            return this;
        }

        internal string TextInternal => string.Join(",", Parameters.Select(parameter => parameter.FormatArgumentPair(enclose: false)));
    }
}