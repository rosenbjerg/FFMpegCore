using FFMpegCore.FFMPEG.Pipes;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Argument
{
    public class OutputPipeArgument : PipeArgument
    {
        public IPipeDataReader Reader { get; private set; }

        public OutputPipeArgument(IPipeDataReader reader) : base(PipeDirection.In)
        {
            Reader = reader;
        }

        public override string GetStringValue()
        {
            return $"\"{PipePath}\" -y";
        }

        public override async Task ProcessDataAsync(CancellationToken token)
        {
            await Pipe.WaitForConnectionAsync(token).ConfigureAwait(false);
            if (!Pipe.IsConnected)
                throw new TaskCanceledException();
            await Reader.ReadDataAsync(Pipe).ConfigureAwait(false);
        }
    }
}
