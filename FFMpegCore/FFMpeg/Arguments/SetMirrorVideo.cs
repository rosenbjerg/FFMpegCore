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
                    case Mirror.Horizontall:
                        return "hflip";
                    case Mirror.Verticall:
                        return "vflip";
                    default:
                        throw new Exception("SetMirrorVideo: argument not found");
                }

            }
        }
    }
}
