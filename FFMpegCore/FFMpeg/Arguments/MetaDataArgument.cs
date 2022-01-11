using FFMpegCore.Extend;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Arguments
{
    public class MetaDataArgument : IInputArgument, IDynamicArgument
    {
        private readonly string _metaDataContent;
        private readonly string _tempFileName = Path.Combine(GlobalFFOptions.Current.TemporaryFilesFolder, $"metadata_{Guid.NewGuid()}.txt");

        public MetaDataArgument(string metaDataContent)
        {
            _metaDataContent = metaDataContent;
        }

        public string Text => GetText(null);

        public Task During(CancellationToken cancellationToken = default) => Task.CompletedTask;


        public void Pre() => File.WriteAllText(_tempFileName, _metaDataContent);

        public void Post() => File.Delete(_tempFileName);

        public string GetText(StringBuilder context)
        {
            var index = context?.ToString().CountOccurrences("-i") ?? 0;

            return $"-i \"{_tempFileName}\" -map_metadata {index}";
        }
    }
}
