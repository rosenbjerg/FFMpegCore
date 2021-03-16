using System;
using System.Collections.Generic;
using System.Text;

namespace FFMpegCore.Arguments
{
    public class SetMirrorVideo : IArgument
    {
        public string Text { get; set; } = "-vf \"hflip\"";
    }
}
