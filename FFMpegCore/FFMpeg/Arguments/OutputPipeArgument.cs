using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore.Pipes;

namespace FFMpegCore.Arguments
{
    public class OutputPipeArgument : PipeArgument, IOutputArgument
    {
        public readonly IPipeSink Reader;

        public OutputPipeArgument(IPipeSink reader) : base(PipeDirection.In)
        {
            Reader = reader;
        }

        public override string Text => $"\"{PipePath}\" -y";

        protected override async Task ProcessDataAsync(CancellationToken token)
        {
            await Pipe.WaitForConnectionAsync(token).ConfigureAwait(false);
            if (!Pipe.IsConnected)
                throw new TaskCanceledException();
            await Reader.ReadAsync(Pipe, token).ConfigureAwait(false);
        }
    }
}
