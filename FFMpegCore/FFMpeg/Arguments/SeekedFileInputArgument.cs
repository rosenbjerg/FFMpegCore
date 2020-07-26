using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Arguments
{
    public class SeekedFileInputArgument : IInputArgument
    {
        public readonly (string FilePath, TimeSpan StartTime)[] SeekedFiles;

        public SeekedFileInputArgument((string file, TimeSpan startTime)[] seekedFiles)
        {
            SeekedFiles = seekedFiles;
        }

        public void Pre()
        {
            foreach (var (seekedFile, _) in SeekedFiles)
            {
                if (!File.Exists(seekedFile))
                    throw new FileNotFoundException("Input file not found", seekedFile);
            }
        }
        public Task During(CancellationToken? cancellationToken = null) => Task.CompletedTask;
        public void Post() { }
        
        public string Text => string.Join(" ", SeekedFiles.Select(seekedFile => $"-ss {seekedFile.StartTime} -i \"{seekedFile.FilePath}\""));
    }
}