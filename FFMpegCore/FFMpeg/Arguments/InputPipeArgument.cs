using FFMpegCore.FFMPEG.Pipes;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents input parameter for a named pipe
    /// </summary>
    public class InputPipeArgument : PipeArgument
    {
        public readonly IPipeDataWriter Writer;

        public InputPipeArgument(IPipeDataWriter writer) : base(PipeDirection.Out)
        {
            Writer = writer;
        }

        public override string Text => $"-y {Writer.GetFormat()} -i \"{PipePath}\"";

        public override async Task ProcessDataAsync(CancellationToken token)
        {
            await Pipe.WaitForConnectionAsync(token).ConfigureAwait(false);
            if (!Pipe.IsConnected)
                throw new TaskCanceledException();
            await Writer.WriteDataAsync(Pipe).ConfigureAwait(false);
        }
    }
}
