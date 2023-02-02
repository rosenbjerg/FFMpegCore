using FFMpegCore.Extend;

namespace FFMpegCore.Arguments
{
    public class PadArgument : IVideoFilterArgument
    {
        private readonly PadOptions _options;

        public PadArgument(PadOptions options)
        {
            _options = options;
        }

        public string Key => "pad";
        public string Value => _options.TextInternal;

    }

    public class PadOptions
    {
        public readonly Dictionary<string, string> Parameters = new();

        internal string TextInternal => string.Join(":", Parameters.Select(parameter => parameter.FormatArgumentPair(true)));

        public static PadOptions Create(string? width, string? height)
        {
            return new PadOptions(width, height);
        }

        public static PadOptions Create(string aspectRatio)
        {
            return new PadOptions(aspectRatio);
        }

        public PadOptions WithParameter(string key, string value)
        {
            Parameters.Add(key, value);
            return this;
        }

        private PadOptions(string? width, string? height)
        {
            if (width == null && height == null)
            {
                throw new Exception("At least one of the parameters must be not null");
            }

            if (width != null)
            {
                Parameters.Add("width", width);
            }

            if (height != null)
            {
                Parameters.Add("height", height);
            }
        }

        private PadOptions(string aspectRatio)
        {
            Parameters.Add("aspect", aspectRatio);
        }
    }
}
