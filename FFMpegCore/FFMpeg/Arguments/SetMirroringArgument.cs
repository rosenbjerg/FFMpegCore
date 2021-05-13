using FFMpegCore.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFMpegCore.Arguments
{
    public class SetMirroringArgument : IVideoFilterArgument
    {
        public SetMirroringArgument(Mirroring mirroring)
        {
            Mirroring = mirroring;
        }

        public Mirroring Mirroring { get; set; }

        public string Key => string.Empty;

        public string Value
        {
            get
            {
                return Mirroring switch
                {
                    Mirroring.Horizontal => "hflip",
                    Mirroring.Vertical => "vflip",
                    _ => throw new ArgumentOutOfRangeException(nameof(Mirroring))
                };
            }
        }
    }
}
