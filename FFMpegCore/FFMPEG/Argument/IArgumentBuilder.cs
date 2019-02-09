using FFMpegCore.FFMPEG.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Argument
{
    public interface IArgumentBuilder
    {
        string BuildArguments(ArgumentContainer container);
    }
}
