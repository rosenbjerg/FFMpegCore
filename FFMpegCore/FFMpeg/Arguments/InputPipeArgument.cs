using System.IO.Pipes;
using FFMpegCore.Pipes;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents input parameter for a named pipe
/// </summary>
public class InputPipeArgument : PipeArgument, IInputArgument
{
    public readonly IPipeSource Writer;

    public InputPipeArgument(IPipeSource writer) : base(PipeDirection.Out)
    {
        Writer = writer;
    }

    public override string Text
    {
        get
        {
            var streamArguments = Writer.GetStreamArguments();
            return string.IsNullOrEmpty(streamArguments) ? $"-i \"{PipePath}\"" : $"{streamArguments} -i \"{PipePath}\"";
        }
    }

    protected override async Task ProcessDataAsync(CancellationToken token)
    {
        await Pipe.WaitForConnectionAsync(token).ConfigureAwait(false);
        if (!Pipe.IsConnected)
        {
            throw new OperationCanceledException();
        }

        await Writer.WriteAsync(Pipe, token).ConfigureAwait(false);
    }
}
