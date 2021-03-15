using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents output stream parameter
    /// </summary>
    public class OutputStreamArgument : IOutputArgument
    {
        public readonly string Stream;

        public OutputStreamArgument(string stream)
        {
            Stream = stream;
        }

        public void Post() { }

        public Task During(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Pre() { }

        public string Text => Stream;
    }
}
