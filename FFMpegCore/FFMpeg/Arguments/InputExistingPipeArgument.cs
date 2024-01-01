using System.Diagnostics;
using FFMpegCore.Pipes;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents input parameter for a specific name pipe
    /// </summary>
    public class InputExistingPipeArgument : IInputArgument
    {
        public string PipeName { get; }
        public string PipePath => PipeHelpers.GetPipePath(PipeName);
        public string Text => $"-i \"{PipePath}\"";

        public InputExistingPipeArgument(string pipeName)
        {
            PipeName = pipeName;
        }

        public void Pre()
        {
            if (string.IsNullOrEmpty(PipeName))
            {
                throw new InvalidOperationException("Pipe name cannot be null or empty");
            }
        }

        public async Task During(CancellationToken cancellationToken = default)
        {
            try
            {
                var tcs = new TaskCompletionSource<bool>();
                cancellationToken.Register(() => tcs.TrySetCanceled());
                await tcs.Task;
            }
            catch(OperationCanceledException)
            {
                Debug.WriteLine($"{GetType().Name} cancelled");
            }
        }

        public void Post()
        {
        }
    }
}
