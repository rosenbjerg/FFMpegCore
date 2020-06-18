using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Arguments
{
    public interface IInputOutputArgument : IArgument
    {
        void Pre() {}
        Task During(CancellationToken? cancellationToken = null) => Task.CompletedTask;
        void Post() {}
    }
}