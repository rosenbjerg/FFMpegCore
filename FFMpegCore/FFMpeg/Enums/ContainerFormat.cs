using System.Text.RegularExpressions;

namespace FFMpegCore.Enums
{
    public class ContainerFormat
    {
        private static readonly Regex _formatRegex = new Regex(@"([D ])([E ])\s+([a-z0-9_]+)\s+(.+)");

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
            var match = _formatRegex.Match(line);
            if (!match.Success)
            {
                fmt = null!;
                return false;
            }

            fmt = new ContainerFormat(match.Groups[3].Value);
            fmt.DemuxingSupported = match.Groups[1].Value == " ";
            fmt.MuxingSupported = match.Groups[2].Value == " ";
            fmt.Description = match.Groups[4].Value;
            return true;
        }
    }
}
