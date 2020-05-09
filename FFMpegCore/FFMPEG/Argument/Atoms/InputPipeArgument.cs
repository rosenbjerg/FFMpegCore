using FFMpegCore.FFMPEG.Pipes;
using Instances;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents input parameter for a named pipe
    /// </summary>
    public class InputPipeArgument : PipeArgument
    {
        public IPipeDataWriter Writer { get; private set; }

        public InputPipeArgument(IPipeDataWriter writer) : base(PipeDirection.Out)
        {
            Writer = writer;
        }

        public override string GetStringValue()
        {
            return $"-y {Writer.GetFormat()} -i \"{PipePath}\"";
        }

        public override async Task ProcessDataAsync(CancellationToken token)
        {
            await Pipe.During(token).ConfigureAwait(false);
            await Writer.WriteDataAsync(Pipe.GetStream()).ConfigureAwait(false);
        }
    }
}
