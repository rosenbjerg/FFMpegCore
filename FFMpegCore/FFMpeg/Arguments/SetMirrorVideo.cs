using FFMpegCore.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFMpegCore.Arguments
{
    public class SetMirrorVideo : IVideoFilterArgument
    {
        public SetMirrorVideo(Mirror value)
        {
            _value = value;
        }

        public Mirror _value { get; set; }

        public string Key => string.Empty;

        public string Value
        {
            get
            {
                switch (_value)
                {
                    case Mirror.Horizontal:
                        return "hflip";
                    case Mirror.Vertical:
                        return "vflip";
                    default:
                        throw new ArgumentOutOfRangeException("SetMirrorVideo: argument not found");
                }

            }
        }
    }
}
