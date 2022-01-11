using FFMpegCore.Extend;

using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Arguments
{
    public class MapMetadataArgument : IInputArgument, IDynamicArgument
    {
        private readonly int? _inputIndex;

        /// <summary>
        /// Null means it takes the last input used befroe this argument
        /// </summary>
        /// <param name="inputIndex"></param>
        public MapMetadataArgument(int? inputIndex = null)
        {
            _inputIndex = inputIndex;
        }

        public string Text => GetText(null);

        public string GetText(StringBuilder context)
        {
            var index = _inputIndex ?? context?.ToString().CountOccurrences("-i") -1 ?? 0;
            return $"-map_metadata {index}";
        }


        public Task During(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Post()
        {
        }

        public void Pre()
        {
        }
    }
}
