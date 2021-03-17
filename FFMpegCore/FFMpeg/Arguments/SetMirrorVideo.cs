using FFMpegCore.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFMpegCore.Arguments
{
    public class SetMirrorVideo : IArgument
    {
        public SetMirrorVideo(Mirror value)
        {
            _value = value;
        }

        public Mirror _value { get; set; }

        public string Text
        {
            get
            {
                switch (_value)
                {
                    case Mirror.Horizontall:
                        return "-vf \"hflip\"";
                    case Mirror.Verticall:
                        return "-vf \"vflip\"";
                    default:
                        throw new Exception("SetMirrorVideo: argument not found");
                }

            }
        }
    }
}
