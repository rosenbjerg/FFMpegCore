using System.Text.RegularExpressions;

namespace FFMpegCore.Enums
{
    public class ContainerFormat
    {
        private static readonly Regex FormatRegex = new Regex(@"([D ])([E ])\s+([a-z0-9_]+)\s+(.+)");

        public string Name { get; private set; }
        public bool DemuxingSupported { get; private set; }
        public bool MuxingSupported { get; private set; }
        public string Description { get; private set; } = null!;

        public string Extension
        {
            get
            {
                if (FFMpegOptions.Options.ExtensionOverrides.ContainsKey(Name))
                    return FFMpegOptions.Options.ExtensionOverrides[Name];
                return "." + Name;
            }
        }

        internal ContainerFormat(string name)
        {
            Name = name;
        }

        internal static bool TryParse(string line, out ContainerFormat fmt)
        {
            var match = FormatRegex.Match(line);
            if (!match.Success)
            {
                fmt = null!;
                return false;
            }

            fmt = new ContainerFormat(match.Groups[3].Value)
            {
                DemuxingSupported = match.Groups[1].Value != " ",
                MuxingSupported = match.Groups[2].Value != " ",
                Description = match.Groups[4].Value
            };
            return true;
        }
    }
}