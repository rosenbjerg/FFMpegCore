using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore.Pipes;

namespace FFMpegCore.Arguments
{
    public abstract class PipeArgument : IInputArgument, IOutputArgument
    {
        private string PipeName { get; }
        public string PipePath => PipeHelpers.GetPipePath(PipeName);

        protected NamedPipeServerStream Pipe { get; private set; } = null!;
        private readonly PipeDirection _direction;

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
            Pipe = null!;
        }

        public async Task During(CancellationToken? cancellationToken = null)
        {
            await ProcessDataAsync(cancellationToken ?? CancellationToken.None)
                .ContinueWith(task =>
                {
                    Post();
                    if (task.Exception != null)
                        throw task.Exception;
                }).ConfigureAwait(false);
        }

        public abstract Task ProcessDataAsync(CancellationToken token);
        public abstract string Text { get; }
    }
}
