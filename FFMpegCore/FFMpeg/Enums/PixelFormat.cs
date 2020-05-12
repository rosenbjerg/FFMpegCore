using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FFMpegCore.Enums
{
    public class PixelFormat
    {
        private static readonly Regex _formatRegex = new Regex(@"([I\.])([O\.])([H\.])([P\.])([B\.])\s+(\S+)\s+([0-9]+)\s+([0-9]+)");

        public bool InputConversionSupported { get; private set; }
        public bool OutputConversionSupported { get; private set; }
        public bool HardwareAccelerationSupported { get; private set; }
        public bool IsPaletted { get; private set; }
        public bool IsBitstream { get; private set; }
        public string Name { get; private set; }
        public int Components { get; private set; }
        public int BitsPerPixel { get; private set; }

        public bool CanConvertTo(PixelFormat other)
        {
            return InputConversionSupported && other.OutputConversionSupported;
        }

        internal PixelFormat(string name)
        {
            Name = name;
        }

        internal static bool TryParse(string line, out PixelFormat fmt)
        {
            var match = _formatRegex.Match(line);
            if (!match.Success)
            {
                fmt = null!;
                return false;
            }

            fmt = new PixelFormat(match.Groups[6].Value);
            fmt.InputConversionSupported = match.Groups[1].Value != ".";
            fmt.OutputConversionSupported = match.Groups[2].Value != ".";
            fmt.HardwareAccelerationSupported = match.Groups[3].Value != ".";
            fmt.IsPaletted = match.Groups[4].Value != ".";
            fmt.IsBitstream = match.Groups[5].Value != ".";
            if (!int.TryParse(match.Groups[7].Value, out var nbComponents))
                return false;
            fmt.Components = nbComponents;
            if (!int.TryParse(match.Groups[8].Value, out var bpp))
                return false;
            fmt.BitsPerPixel = bpp;

            return true;
        }
    }
}
