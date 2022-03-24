using FFMpegCore.Extend;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Arguments
{
    public class MapMetadataArgument : IInputArgument, IDynamicArgument
    {
        private readonly int? _inputIndex;

        public string Text => GetText(null);

        /// <summary>
        /// Null means it takes the last input used before this argument
        /// </summary>
        /// <param name="inputIndex"></param>
        public MapMetadataArgument(int? inputIndex = null)
        {
            _inputIndex = inputIndex;
        }

        public string GetText(IEnumerable<IArgument>? arguments)
        {
            arguments ??= Enumerable.Empty<IArgument>();

            var index = 0;
            if (_inputIndex is null)
            {
                index = arguments
                   .TakeWhile(x => x != this)
                   .OfType<IInputArgument>()
                   .Count();

                index = Math.Max(index - 1, 0);
            }
            else
            {
                index = _inputIndex.Value;
            }

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
