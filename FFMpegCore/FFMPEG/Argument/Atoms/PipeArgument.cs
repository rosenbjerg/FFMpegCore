using FFMpegCore.FFMPEG.Pipes;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Argument
{
    public abstract class PipeArgument : Argument
    {
        public string PipeName { get; private set; }
        public string PipePath => PipeHelpers.GetPipePath(PipeName);

        protected NamedPipeServerStream Pipe { get; private set; }
        private PipeDirection direction;

        protected PipeArgument(PipeDirection direction)
        {
            PipeName = PipeHelpers.GetUnqiuePipeName();
            this.direction = direction;
        }

        public void OpenPipe()
        {
            if (Pipe != null)
                throw new InvalidOperationException("Pipe already has been opened");

            Pipe = new NamedPipeServerStream(PipeName, direction, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }

        public void ClosePipe()
        {
            Pipe?.Dispose();
            Pipe = null;
        }
        public async Task ProcessDataAsync()
        {
            await ProcessDataAsync(CancellationToken.None).ConfigureAwait(false);
        }

        public abstract Task ProcessDataAsync(CancellationToken token);
    }
}
