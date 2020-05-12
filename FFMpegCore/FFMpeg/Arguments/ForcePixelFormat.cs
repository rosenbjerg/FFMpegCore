using FFMpegCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFMpegCore.Arguments
{
    public class ForcePixelFormat : IArgument
    {
        public string PixelFormat { get; private set; }
        public string Text => $"-pix_fmt {PixelFormat}";

        public ForcePixelFormat(string format)
        {
            PixelFormat = format;
        }

        public ForcePixelFormat(PixelFormat format) : this(format.Name) { }
    }
}
