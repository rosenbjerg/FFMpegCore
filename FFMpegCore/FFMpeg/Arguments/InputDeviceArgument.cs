using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents an input device parameter
    /// </summary>
    public class InputDeviceArgument : IInputArgument
    {
        private readonly string Device;

        public InputDeviceArgument(string device)
        {
            Device = device;
        }

        public Task During(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Pre() { }

        public void Post() { }

        public string Text => $"-i {Device}";
    }
}
