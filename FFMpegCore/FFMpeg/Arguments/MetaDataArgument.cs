using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Arguments
{
    public class MetaDataArgument : IInputArgument
    {
        private readonly string _metaDataContent;
        private readonly string _tempFileName = Path.Combine(GlobalFFOptions.Current.TemporaryFilesFolder, $"metadata_{Guid.NewGuid()}.txt");

        public MetaDataArgument(string metaDataContent)
        {
            _metaDataContent = metaDataContent;
        }

        public string Text => $"-i \"{_tempFileName}\" -map_metadata 1";

        public Task During(CancellationToken cancellationToken = default) => Task.CompletedTask;


        public void Pre() => File.WriteAllText(_tempFileName, _metaDataContent);

        public void Post() => File.Delete(_tempFileName);
    }
}
