using System.IO.Pipes;
using FFMpegCore.Pipes;

namespace FFMpegCore.Arguments;

public class OutputPipeArgument : PipeArgument, IOutputArgument
{
    public readonly IPipeSink Reader;

    public OutputPipeArgument(IPipeSink reader) : base(PipeDirection.In)
    {
        Reader = reader;
    }

    public override string Text
    {
        get
        {
            var streamArguments = Reader.GetStreamArguments();
            return string.IsNullOrEmpty(streamArguments) ? $"\"{PipePath}\" -y" : $"{streamArguments} \"{PipePath}\" -y";
        }
    }

    protected override async Task ProcessDataAsync(CancellationToken token)
    {
        await Pipe.WaitForConnectionAsync(token).ConfigureAwait(false);
        if (!Pipe.IsConnected)
        {
            throw new TaskCanceledException();
        }

        await Reader.ReadAsync(Pipe, token).ConfigureAwait(false);
    }
}
