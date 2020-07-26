using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Arguments
{
    public class SeekedFileInputArgument : IInputArgument
    {
        public readonly string FilePath;
        public readonly TimeSpan StartTime;

        public SeekedFileInputArgument(string filePath, TimeSpan startTime)
        {
            FilePath = filePath;
            StartTime = startTime;
        }
        public void Pre()
        {
            if (!File.Exists(FilePath))
                throw new FileNotFoundException("Input file not found", FilePath);
        }
        public Task During(CancellationToken? cancellationToken = null) => Task.CompletedTask;
        public void Post() { }
        
        public string Text => $"-ss {StartTime} -i \"{FilePath}\"";
    }
}