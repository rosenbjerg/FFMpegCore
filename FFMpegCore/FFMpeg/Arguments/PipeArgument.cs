using FFMpegCore.FFMPEG.Pipes;
using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Argument
{
    public abstract class PipeArgument : IInputArgument, IOutputArgument
    {
        private string PipeName { get; }
        public string PipePath => PipeHelpers.GetPipePath(PipeName);

        protected NamedPipeServerStream Pipe { get; private set; }
        private PipeDirection _direction;

        protected PipeArgument(PipeDirection direction)
        {
            PipeName = PipeHelpers.GetUnqiuePipeName();
            _direction = direction;
        }

        public void Pre()
        {
            if (Pipe != null)
                throw new InvalidOperationException("Pipe already has been opened");

            Pipe = new NamedPipeServerStream(PipeName, _direction, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }

        public void Post()
        {
            Pipe?.Dispose();
            Pipe = null;
        }

        public Task During(CancellationToken? cancellationToken = null)
        {
            return ProcessDataAsync(cancellationToken ?? CancellationToken.None);
        }

        public abstract Task ProcessDataAsync(CancellationToken token);
        public abstract string Text { get; }
    }
}
