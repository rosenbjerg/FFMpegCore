using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore.FFMPEG.Pipes;

namespace FFMpegCore.FFMPEG.Argument
{
    public class OutputPipeArgument : PipeArgument
    {
        public readonly IPipeDataReader Reader;

        public OutputPipeArgument(IPipeDataReader reader) : base(PipeDirection.In)
        {
            Reader = reader;
        }

        public override string Text => $"\"{PipePath}\" -y";

        public override async Task ProcessDataAsync(CancellationToken token)
        {
            await Pipe.WaitForConnectionAsync(token).ConfigureAwait(false);
            if (!Pipe.IsConnected)
                throw new TaskCanceledException();
            await Reader.ReadDataAsync(Pipe).ConfigureAwait(false);
        }
    }
}
